--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: art_market_schema; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA art_market_schema;


ALTER SCHEMA art_market_schema OWNER TO postgres;

--
-- Name: check_stock_on_insert(); Type: FUNCTION; Schema: art_market_schema; Owner: postgres
--

CREATE FUNCTION art_market_schema.check_stock_on_insert() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    is_individual BOOLEAN;
    available_qty INTEGER;
BEGIN
    SELECT (id_indiv_buyer IS NOT NULL), quantity_for_sale
    INTO is_individual, available_qty
    FROM art_market_schema.product
    WHERE id_product = NEW.id_product;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'PRODUCT_NOT_EXISTS: Товар с ID % не существует.', NEW.id_product;
    END IF;

    IF is_individual THEN
        RETURN NEW; -- для индивидуальных проверка не нужна
    END IF;

    IF available_qty < NEW.quantity_in_purchase THEN
        RAISE EXCEPTION 'INSUFFICIENT_STOCK: Недостаточно товара "%". Доступно: %, запрошено: %', (SELECT name FROM art_market_schema.product WHERE id_product = NEW.id_product), available_qty, NEW.quantity_in_purchase;
    END IF;

    RETURN NEW;
END;
$$;


ALTER FUNCTION art_market_schema.check_stock_on_insert() OWNER TO postgres;

--
-- Name: create_custom_product(integer, character varying, character varying, numeric); Type: PROCEDURE; Schema: art_market_schema; Owner: postgres
--

CREATE PROCEDURE art_market_schema.create_custom_product(IN p_order_id integer, IN p_name character varying, IN p_type_art character varying, IN p_price numeric)
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_seller_id INTEGER;
    v_buyer_id INTEGER;
    v_product_id INTEGER;
BEGIN
    IF p_name IS NULL OR LENGTH(TRIM(p_name)) = 0 THEN
        RAISE EXCEPTION 'PRODUCT_NAME_REQUIRED: Имя товара обязательно.';
    END IF;

    IF LENGTH(p_name) > 100 THEN
        RAISE EXCEPTION 'PRODUCT_NAME_TOO_LONG: Имя товара не может превышать 100 символов.';
    END IF;

    IF p_type_art IS NULL OR LENGTH(TRIM(p_type_art)) = 0 THEN
        RAISE EXCEPTION 'TYPE_ART_REQUIRED: Тип искусства обязателен.';
    END IF;
    
    IF LENGTH(p_type_art) > 100 THEN
        RAISE EXCEPTION 'TYPE_ART_TOO_LONG: Тип искусства не может превышать 100 символов.';
    END IF;

    IF p_price <= 0 THEN
        RAISE EXCEPTION 'INVALID_PRICE: Цена должна быть больше 0.';
    END IF;
	 
    -- Получаем ID продавца и покупателя из заказа на изготовление
    SELECT id_seller, id_buyer
    INTO v_seller_id, v_buyer_id
    FROM art_market_schema.production_purchase
    WHERE id_production_purchase = p_order_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'ORDER_NOT_FOUND: Заказ на изготовление с ID % не найден.', p_order_id;
    END IF;

    -- Создаём новый индивидуальный товар
    INSERT INTO art_market_schema.product (
        name, type_art, id_seller, id_indiv_buyer,
        quantity_for_sale, price, status
    ) VALUES (
        p_name, p_type_art, v_seller_id, v_buyer_id,
        1, p_price, 'reserved'
    )
    RETURNING id_product INTO v_product_id;

    -- связываем заказ с созданным товаром
    UPDATE art_market_schema.production_purchase
    SET id_product = v_product_id
    WHERE id_production_purchase = p_order_id;

    EXCEPTION
    WHEN UNIQUE_VIOLATION THEN
        RAISE EXCEPTION 'DUPLICATE_PRODUCT_DATA: Нарушена уникальность данных';
    WHEN OTHERS THEN
        RAISE EXCEPTION 'UNEXPECTED_ERROR: Ошибка при создании товара: %', SQLERRM;

END;
$$;


ALTER PROCEDURE art_market_schema.create_custom_product(IN p_order_id integer, IN p_name character varying, IN p_type_art character varying, IN p_price numeric) OWNER TO postgres;

--
-- Name: deduct_stock_on_accept(); Type: FUNCTION; Schema: art_market_schema; Owner: postgres
--

CREATE FUNCTION art_market_schema.deduct_stock_on_accept() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    is_individual BOOLEAN;
    current_qty INTEGER;
    new_qty INTEGER;
BEGIN
    SELECT (id_indiv_buyer IS NOT NULL), quantity_for_sale
    INTO is_individual, current_qty
    FROM art_market_schema.product
    WHERE id_product = NEW.id_product;

    IF is_individual THEN
        RETURN NEW;
    END IF;

    IF current_qty < NEW.quantity_in_purchase THEN
        RAISE EXCEPTION 'INSUFFICIENT_STOCK: Недостаточно товара "%". Доступно: % шт., запрошено: % шт.', NEW.id_product, current_qty, NEW.quantity_in_purchase;
    END IF;

    new_qty := current_qty - NEW.quantity_in_purchase;
    
-- Обновляем количество и статус
    UPDATE art_market_schema.product
    SET 
        quantity_for_sale = new_qty,
        status = CASE WHEN new_qty = 0 THEN 'sold' ELSE 'available' END
    WHERE id_product = NEW.id_product;

    RETURN NEW;
END;
$$;


ALTER FUNCTION art_market_schema.deduct_stock_on_accept() OWNER TO postgres;

--
-- Name: get_materials_for_product(integer); Type: FUNCTION; Schema: art_market_schema; Owner: postgres
--

CREATE FUNCTION art_market_schema.get_materials_for_product(p_product_id integer) RETURNS TABLE(material_name character varying, quantity numeric, unit character varying, total_cost numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF p_product_id IS NULL THEN
        RAISE EXCEPTION 'PRODUCT_ID_REQUIRED: Необходимо указать ID товара.';
    END IF;

    PERFORM 1 FROM art_market_schema.product WHERE id_product = p_product_id;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'PRODUCT_NOT_FOUND: Товар с ID % не существует.', p_product_id;
    END IF;

    RETURN QUERY
    SELECT
        m.material_name,
        cpm.quantity,
        cpm.unit,
        cpm.quantity * m.cost_per_unit AS total_cost
    FROM art_market_schema.connect_product_material cpm
    JOIN art_market_schema.catalog_for_material m ON cpm.id_material = m.id_material
    WHERE cpm.id_product = p_product_id;
END;
$$;


ALTER FUNCTION art_market_schema.get_materials_for_product(p_product_id integer) OWNER TO postgres;

--
-- Name: log_product_cost(); Type: FUNCTION; Schema: art_market_schema; Owner: postgres
--

CREATE FUNCTION art_market_schema.log_product_cost() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    target_product_id INTEGER;
    total NUMERIC(12,2);
BEGIN
    -- Определяем ID продукта в зависимости от типа операции
    IF TG_OP = 'DELETE' THEN
        target_product_id := OLD.ID_product;
    ELSE
        target_product_id := NEW.ID_product;
    END IF;
	
    IF target_product_id IS NULL THEN
        RETURN NULL; 
    END IF;

    -- Рассчитываем общую себестоимость изделия
    SELECT COALESCE(SUM(cpm.quantity * m.cost_per_unit), 0)
    INTO total
    FROM art_market_schema.connect_product_material cpm
    JOIN art_market_schema.catalog_for_material m ON cpm.ID_material = m.ID_material
    WHERE cpm.ID_product = target_product_id;

    -- Записываем в лог
    INSERT INTO product_cost_log (ID_product, total_cost)
    VALUES (target_product_id, total);

    -- Возвращаем корректное значение в зависимости от операции
    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    ELSE
        RETURN NEW;
    END IF;
END;
$$;


ALTER FUNCTION art_market_schema.log_product_cost() OWNER TO postgres;

--
-- Name: sales_by_art_type(date, date); Type: FUNCTION; Schema: art_market_schema; Owner: postgres
--

CREATE FUNCTION art_market_schema.sales_by_art_type(start_date date, end_date date) RETURNS TABLE(type_art character varying, total_quantity bigint, total_revenue numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF start_date IS NULL OR end_date IS NULL THEN
        RAISE EXCEPTION 'start_date и end_date не могут быть NULL';
    END IF;

    IF start_date > end_date THEN
        RAISE EXCEPTION 'Дата начала периода не может быть позже даты окончания';
    END IF;

    RETURN QUERY
    SELECT
        p.type_art,
        SUM(ip.quantity_in_purchase) AS total_quantity,
        SUM(ip.price_in_purchase * ip.quantity_in_purchase) AS total_revenue
    FROM art_market_schema.purchase pur
    JOIN item_purchase ip ON pur.id_purchase = ip.id_purchase
    JOIN product p ON ip.id_product = p.id_product
    WHERE pur.created_at BETWEEN start_date AND end_date
      AND ip.status_item IN ('accepted', 'delivered')
    GROUP BY p.type_art;
END;
$$;


ALTER FUNCTION art_market_schema.sales_by_art_type(start_date date, end_date date) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: account; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.account (
    id_account integer NOT NULL,
    account_name character varying(50) NOT NULL,
    address text NOT NULL,
    email character varying(50) NOT NULL,
    phone character varying(11) NOT NULL,
    password character varying(20) NOT NULL,
    id_role integer NOT NULL,
    CONSTRAINT account_account_name_check CHECK (((account_name)::text ~ '^[A-Za-zА-Яа-я0-9\s\-_]+$'::text)),
    CONSTRAINT account_email_check CHECK (((email)::text ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'::text)),
    CONSTRAINT account_phone_check CHECK (((phone)::text ~ '^[0-9]{11}$'::text))
);


ALTER TABLE art_market_schema.account OWNER TO postgres;

--
-- Name: account_id_account_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.account_id_account_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.account_id_account_seq OWNER TO postgres;

--
-- Name: account_id_account_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.account_id_account_seq OWNED BY art_market_schema.account.id_account;


--
-- Name: catalog_for_material; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.catalog_for_material (
    id_material integer NOT NULL,
    material_name character varying(100) NOT NULL,
    cost_per_unit numeric(10,2) NOT NULL,
    CONSTRAINT catalog_for_material_cost_per_unit_check CHECK ((cost_per_unit >= (0)::numeric)),
    CONSTRAINT catalog_for_material_material_name_check CHECK (((material_name)::text ~ '^[A-Za-zА-Яа-я0-9\s\-]+$'::text))
);


ALTER TABLE art_market_schema.catalog_for_material OWNER TO postgres;

--
-- Name: catalog_for_material_id_material_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.catalog_for_material_id_material_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.catalog_for_material_id_material_seq OWNER TO postgres;

--
-- Name: catalog_for_material_id_material_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.catalog_for_material_id_material_seq OWNED BY art_market_schema.catalog_for_material.id_material;


--
-- Name: connect_product_material; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.connect_product_material (
    id integer NOT NULL,
    id_product integer NOT NULL,
    id_material integer NOT NULL,
    quantity numeric(10,3) NOT NULL,
    unit character varying(20) NOT NULL
);


ALTER TABLE art_market_schema.connect_product_material OWNER TO postgres;

--
-- Name: connect_product_material_id_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.connect_product_material_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.connect_product_material_id_seq OWNER TO postgres;

--
-- Name: connect_product_material_id_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.connect_product_material_id_seq OWNED BY art_market_schema.connect_product_material.id;


--
-- Name: item_purchase; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.item_purchase (
    id_item_purchase integer NOT NULL,
    id_purchase integer NOT NULL,
    id_product integer NOT NULL,
    quantity_in_purchase integer NOT NULL,
    price_in_purchase numeric(12,2) NOT NULL,
    status_item character varying(30) NOT NULL,
    CONSTRAINT item_purchase_quantity_in_purchase_check CHECK (((quantity_in_purchase > 0) AND (quantity_in_purchase <= 100))),
    CONSTRAINT item_purchase_status_item_check CHECK (((status_item)::text = ANY ((ARRAY['pending'::character varying, 'shipped'::character varying, 'delivered'::character varying, 'cancelled'::character varying, 'accepted'::character varying, 'in progress'::character varying])::text[])))
);


ALTER TABLE art_market_schema.item_purchase OWNER TO postgres;

--
-- Name: item_purchase_id_item_purchase_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.item_purchase_id_item_purchase_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.item_purchase_id_item_purchase_seq OWNER TO postgres;

--
-- Name: item_purchase_id_item_purchase_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.item_purchase_id_item_purchase_seq OWNED BY art_market_schema.item_purchase.id_item_purchase;


--
-- Name: product; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.product (
    id_product integer NOT NULL,
    name character varying(100) NOT NULL,
    type_art character varying(100) NOT NULL,
    id_seller integer NOT NULL,
    quantity_for_sale integer NOT NULL,
    price numeric(12,2) NOT NULL,
    status character varying(30) NOT NULL,
    id_indiv_buyer integer,
    CONSTRAINT product_name_check CHECK (((name)::text ~ '^[A-Za-zА-Яа-я0-9\s\-.,!?]+$'::text)),
    CONSTRAINT product_quantity_for_sale_check CHECK (((quantity_for_sale >= 0) AND (quantity_for_sale <= 10000))),
    CONSTRAINT product_status_check CHECK (((status)::text = ANY ((ARRAY['available'::character varying, 'reserved'::character varying, 'sold'::character varying])::text[]))),
    CONSTRAINT product_type_art_check CHECK (((type_art)::text ~ '^[A-Za-zА-Яа-я\s]+$'::text))
);


ALTER TABLE art_market_schema.product OWNER TO postgres;

--
-- Name: product_cost_log; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.product_cost_log (
    log_id integer NOT NULL,
    id_product integer NOT NULL,
    total_cost numeric(12,2) NOT NULL,
    updated_at timestamp without time zone DEFAULT now()
);


ALTER TABLE art_market_schema.product_cost_log OWNER TO postgres;

--
-- Name: product_cost_log_log_id_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.product_cost_log_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.product_cost_log_log_id_seq OWNER TO postgres;

--
-- Name: product_cost_log_log_id_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.product_cost_log_log_id_seq OWNED BY art_market_schema.product_cost_log.log_id;


--
-- Name: product_id_product_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.product_id_product_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.product_id_product_seq OWNER TO postgres;

--
-- Name: product_id_product_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.product_id_product_seq OWNED BY art_market_schema.product.id_product;


--
-- Name: production_purchase; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.production_purchase (
    id_production_purchase integer NOT NULL,
    id_seller integer NOT NULL,
    id_buyer integer NOT NULL,
    direction_from_seller boolean NOT NULL,
    text_accounts text,
    id_product integer,
    CONSTRAINT production_purchase_check CHECK ((id_seller <> id_buyer))
);


ALTER TABLE art_market_schema.production_purchase OWNER TO postgres;

--
-- Name: production_purchase_id_production_purchase_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.production_purchase_id_production_purchase_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.production_purchase_id_production_purchase_seq OWNER TO postgres;

--
-- Name: production_purchase_id_production_purchase_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.production_purchase_id_production_purchase_seq OWNED BY art_market_schema.production_purchase.id_production_purchase;


--
-- Name: purchase; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.purchase (
    id_purchase integer NOT NULL,
    number_purchase character varying(20) NOT NULL,
    id_seller integer NOT NULL,
    id_buyer integer NOT NULL,
    address_departure text NOT NULL,
    address_receiving text NOT NULL,
    method_delivery character varying(50) NOT NULL,
    created_at date DEFAULT CURRENT_DATE NOT NULL,
    CONSTRAINT purchase_method_delivery_check CHECK (((method_delivery)::text = ANY ((ARRAY['courier'::character varying, 'pickup'::character varying, 'mail'::character varying])::text[])))
);


ALTER TABLE art_market_schema.purchase OWNER TO postgres;

--
-- Name: purchase_id_purchase_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.purchase_id_purchase_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.purchase_id_purchase_seq OWNER TO postgres;

--
-- Name: purchase_id_purchase_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.purchase_id_purchase_seq OWNED BY art_market_schema.purchase.id_purchase;


--
-- Name: role; Type: TABLE; Schema: art_market_schema; Owner: postgres
--

CREATE TABLE art_market_schema.role (
    id_role integer NOT NULL,
    role_name character varying(50) NOT NULL,
    CONSTRAINT role_role_name_check CHECK (((role_name)::text = ANY ((ARRAY['seller'::character varying, 'buyer'::character varying, 'admin'::character varying])::text[])))
);


ALTER TABLE art_market_schema.role OWNER TO postgres;

--
-- Name: role_id_role_seq; Type: SEQUENCE; Schema: art_market_schema; Owner: postgres
--

CREATE SEQUENCE art_market_schema.role_id_role_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE art_market_schema.role_id_role_seq OWNER TO postgres;

--
-- Name: role_id_role_seq; Type: SEQUENCE OWNED BY; Schema: art_market_schema; Owner: postgres
--

ALTER SEQUENCE art_market_schema.role_id_role_seq OWNED BY art_market_schema.role.id_role;


--
-- Name: account id_account; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.account ALTER COLUMN id_account SET DEFAULT nextval('art_market_schema.account_id_account_seq'::regclass);


--
-- Name: catalog_for_material id_material; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.catalog_for_material ALTER COLUMN id_material SET DEFAULT nextval('art_market_schema.catalog_for_material_id_material_seq'::regclass);


--
-- Name: connect_product_material id; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.connect_product_material ALTER COLUMN id SET DEFAULT nextval('art_market_schema.connect_product_material_id_seq'::regclass);


--
-- Name: item_purchase id_item_purchase; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.item_purchase ALTER COLUMN id_item_purchase SET DEFAULT nextval('art_market_schema.item_purchase_id_item_purchase_seq'::regclass);


--
-- Name: product id_product; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.product ALTER COLUMN id_product SET DEFAULT nextval('art_market_schema.product_id_product_seq'::regclass);


--
-- Name: product_cost_log log_id; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.product_cost_log ALTER COLUMN log_id SET DEFAULT nextval('art_market_schema.product_cost_log_log_id_seq'::regclass);


--
-- Name: production_purchase id_production_purchase; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.production_purchase ALTER COLUMN id_production_purchase SET DEFAULT nextval('art_market_schema.production_purchase_id_production_purchase_seq'::regclass);


--
-- Name: purchase id_purchase; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.purchase ALTER COLUMN id_purchase SET DEFAULT nextval('art_market_schema.purchase_id_purchase_seq'::regclass);


--
-- Name: role id_role; Type: DEFAULT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.role ALTER COLUMN id_role SET DEFAULT nextval('art_market_schema.role_id_role_seq'::regclass);


--
-- Data for Name: account; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.account (id_account, account_name, address, email, phone, password, id_role) FROM stdin;
3	Иван Петров	г. Казань, ул. Баумана 5	ivan.p@buyer.ru	79345678901	buyer789	2
1	Арт Галерея	г. Москва, ул. Тверская 111	gallery@artmarket.ru	79123456789	1234-505	1
8	АдминОДО	Нету:Р	son@gmail.com	89123310668	Ghost-505	3
9	Admin2	no(	chit@gmail.com	89123310000	ChitCode-505	3
2	Керамика Мастер Коля	г. Санкт-Петербург, Невский пр. 100 	ceramichand@made.ru	79234567811	Keramika-505	1
4	Анна Сидорова	г. Екатеринбург, пр. Ленина 22	anna.s@client.ru	79456789012	anna-505	2
12	Коля Касаткин	город Уфа	kolamarket@gmail.ru	89123310668	KolaMarket-505	2
\.


--
-- Data for Name: catalog_for_material; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.catalog_for_material (id_material, material_name, cost_per_unit) FROM stdin;
2	Масляные краски 1 л	1200.00
3	Бронза 1 кг	50000.00
4	Керамическая глина 1 кг	200.00
5	Глазурь синяя 1 л	300.00
6	глина 	500.00
1	Холст 100x80	600.00
\.


--
-- Data for Name: connect_product_material; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.connect_product_material (id, id_product, id_material, quantity, unit) FROM stdin;
1	1	1	1.000	шт
2	1	2	0.300	л
3	2	3	1.200	кг
4	3	4	1.500	кг
5	3	5	0.100	л
6	4	1	1.000	шт
7	4	2	0.400	л
8	5	4	2.000	кг
9	5	5	0.150	л
13	21	1	1.000	шт
15	23	2	1.000	кг
16	24	2	1.000	кг
17	24	1	3.000	шт
18	25	4	3.000	кг
19	28	2	1.000	кг
20	29	4	1.000	кг
21	30	4	1.000	кг
\.


--
-- Data for Name: item_purchase; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.item_purchase (id_item_purchase, id_purchase, id_product, quantity_in_purchase, price_in_purchase, status_item) FROM stdin;
1	1	1	1	25000.00	accepted
2	2	4	1	35000.00	accepted
12	9	12	5	3500.00	pending
13	10	12	5	3500.00	pending
14	11	12	5	3500.00	pending
15	12	12	4	3500.00	pending
16	13	12	11	3500.00	pending
18	15	12	11	3500.00	pending
19	16	24	3	155000.00	pending
22	19	20	5	1500.00	pending
23	20	12	1	3500.00	pending
24	21	19	45	1000.00	pending
26	23	19	15	1000.00	pending
28	25	12	5	3500.00	pending
29	26	12	3	3500.00	pending
31	28	25	15	1200.00	pending
36	33	29	1	120.00	pending
37	34	29	1	120.00	pending
38	35	29	1	120.00	pending
39	36	29	1	120.00	pending
40	37	29	1	120.00	pending
41	38	29	1	120.00	pending
48	45	29	2	120.00	pending
49	46	26	1	145.00	pending
50	47	26	1	145.00	pending
51	48	26	3	145.00	pending
\.


--
-- Data for Name: product; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.product (id_product, name, type_art, id_seller, quantity_for_sale, price, status, id_indiv_buyer) FROM stdin;
1	Абстрактная композиция	живопись	1	1	25000.00	available	\N
2	Бронзовая скульптура Полет	скульптура	1	1	60000.00	available	\N
3	Керамическая чаша ручной работы	керамика	2	10	3000.00	available	\N
4	Портрет Анны в интерьере	живопись	1	1	35000.00	reserved	4
5	Персональная ваза для Ивана	керамика	2	1	4500.00	reserved	3
7	Ваза малахитовая фарфоровая	фарфор	1	5	5000.00	available	3
8	Ваза песчаная 	скульптура	1	1	100.00	available	3
11	Керамическая рыбка	керамика многовековая	2	3	400000.00	reserved	4
6	Ваза Ручная.	керамика	2	1	8000.00	reserved	3
13	Ваза ктайская Ручная	керамика	2	1	8000.00	reserved	3
18	Супер картина моря	живопись	1	1	120000.00	sold	4
21	Тестовый продукт	тест	1	1	100.00	available	\N
23	Картина моря	живопись	1	12	1500.00	available	\N
24	Картина кота	живопись	1	3	155000.00	available	4
20	Картина моря	живопись	1	7	1500.00	available	\N
19	Картина моря	живопись	1	100	1000.00	available	\N
12	Статуэтка флэшка	скульптура	2	5	3500.00	available	\N
29	статуэтка слона	керамика	2	3	120.00	available	\N
26	статуэтка ящерецы	керамика	2	7	145.00	available	\N
30	статуэтка кита	керамика	2	13	150.00	available	\N
25	Статуэтка кота розовый	керамика	2	135	1200.00	available	\N
28	зеленое пятно	живопись	1	12	12456.00	available	\N
\.


--
-- Data for Name: product_cost_log; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.product_cost_log (log_id, id_product, total_cost, updated_at) FROM stdin;
1	21	600.00	2026-01-19 09:12:18.47169
\.


--
-- Data for Name: production_purchase; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.production_purchase (id_production_purchase, id_seller, id_buyer, direction_from_seller, text_accounts, id_product) FROM stdin;
2	2	3	f	Ваза с гравировкой, синяя глазурь	5
4	2	3	f	Ваза с гравировкой, синяя глазурь	5
7	1	4	f	Нужен портрет в современном стиле	\N
6	2	3	f	Хочу создать индивидуальную керамическую вазу ручной работы\n\n[08.01.2026 20:44] Покупатель:\nХорошо. Из каких материалов?\r\n	\N
8	2	3	f	Хочу вазу ручной работы	\N
5	2	3	f	Хочу вазу ручной работы	13
9	1	3	f	Здравствуйте!\r\n	\N
10	1	3	t	Hi!\r\n	\N
1	1	4	f	Вы можете написать портрет маслом по фотографии?	4
3	1	4	f	Это подарок	4
11	1	4	t	Мы можем это сделать\r\n	\N
12	1	4	f	Спасибо	\N
\.


--
-- Data for Name: purchase; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.purchase (id_purchase, number_purchase, id_seller, id_buyer, address_departure, address_receiving, method_delivery, created_at) FROM stdin;
1	PO-2025-001	1	3	г. Москва, ул. Тверская 15	г. Казань, ул. Баумана 5	courier	2025-11-20
2	PO-2025-002	1	4	г. Москва, ул. Тверская 15	г. Екатеринбург, пр. Ленина 22	pickup	2025-11-22
9	ORD20260110221020431	2	3	г. Санкт-Петербург, Невский пр. 100 	Киров	pickup	2026-01-10
10	ORD20260110225644321	2	3	г. Санкт-Петербург, Невский пр. 100 	Москва	mail	2026-01-10
11	ORD20260110231414514	2	3	г. Санкт-Петербург, Невский пр. 100 	Куда-нибудь 	courier	2026-01-10
12	ORD20260110233050268	2	3	г. Санкт-Петербург, Невский пр. 100 	Киров-Киров	courier	2026-01-10
13	ORD20260111134038281	2	3	г. Санкт-Петербург, Невский пр. 100 	Слобода	mail	2026-01-11
15	ORD20260114143746253	2	3	г. Санкт-Петербург, Невский пр. 100 	улица Колотушкина	courier	2026-01-14
16	ORD20260119213956543	1	4	г. Москва, ул. Тверская 111	Москва ул. Московитовская	courier	2026-01-19
19	ORD20260119214828406	1	4	г. Москва, ул. Тверская 111	Уфа	courier	2026-01-19
20	ORD20260119215102926	2	3	г. Санкт-Петербург, Невский пр. 100 	Уфа	pickup	2026-01-19
21	ORD20260119215144489	1	3	г. Москва, ул. Тверская 111	Уфа	courier	2026-01-19
23	ORD20260119221959666	1	4	г. Москва, ул. Тверская 111	Екатеринбург	mail	2026-01-19
25	ORD20260120141706816	2	4	г. Санкт-Петербург, Невский пр. 100 	Москва	courier	2026-01-20
26	ORD20260120141805399	2	4	г. Санкт-Петербург, Невский пр. 100 	Москва	courier	2026-01-20
28	ORD20260120143837901	2	4	г. Санкт-Петербург, Невский пр. 100 	Москва	courier	2026-01-20
33	ORD20260121001503948	2	4	г. Санкт-Петербург, Невский пр. 100 	Москва	courier	2026-01-21
34	ORD20260121002051331	2	4	г. Санкт-Петербург, Невский пр. 100 	-	courier	2026-01-21
35	ORD20260121002139702	2	4	г. Санкт-Петербург, Невский пр. 100 	-	courier	2026-01-21
36	ORD20260121002217509	2	4	г. Санкт-Петербург, Невский пр. 100 	-	pickup	2026-01-21
37	ORD20260121002335719	2	4	г. Санкт-Петербург, Невский пр. 100 	-	courier	2026-01-21
38	ORD20260121002345112	2	3	г. Санкт-Петербург, Невский пр. 100 	-	pickup	2026-01-21
45	ORD20260121005537874	2	4	г. Санкт-Петербург, Невский пр. 100 	-	courier	2026-01-21
46	ORD20260121010916023	2	4	г. Санкт-Петербург, Невский пр. 100 	Уфа	courier	2026-01-21
47	ORD20260121011030519	2	4	г. Санкт-Петербург, Невский пр. 100 	Уфа	courier	2026-01-21
48	ORD20260121011202972	2	4	г. Санкт-Петербург, Невский пр. 100 	Москва	courier	2026-01-21
\.


--
-- Data for Name: role; Type: TABLE DATA; Schema: art_market_schema; Owner: postgres
--

COPY art_market_schema.role (id_role, role_name) FROM stdin;
1	seller
2	buyer
3	admin
\.


--
-- Name: account_id_account_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.account_id_account_seq', 15, true);


--
-- Name: catalog_for_material_id_material_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.catalog_for_material_id_material_seq', 7, true);


--
-- Name: connect_product_material_id_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.connect_product_material_id_seq', 21, true);


--
-- Name: item_purchase_id_item_purchase_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.item_purchase_id_item_purchase_seq', 51, true);


--
-- Name: product_cost_log_log_id_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.product_cost_log_log_id_seq', 1, true);


--
-- Name: product_id_product_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.product_id_product_seq', 30, true);


--
-- Name: production_purchase_id_production_purchase_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.production_purchase_id_production_purchase_seq', 12, true);


--
-- Name: purchase_id_purchase_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.purchase_id_purchase_seq', 48, true);


--
-- Name: role_id_role_seq; Type: SEQUENCE SET; Schema: art_market_schema; Owner: postgres
--

SELECT pg_catalog.setval('art_market_schema.role_id_role_seq', 3, true);


--
-- Name: account account_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.account
    ADD CONSTRAINT account_pkey PRIMARY KEY (id_account);


--
-- Name: catalog_for_material catalog_for_material_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.catalog_for_material
    ADD CONSTRAINT catalog_for_material_pkey PRIMARY KEY (id_material);


--
-- Name: connect_product_material connect_product_material_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.connect_product_material
    ADD CONSTRAINT connect_product_material_pkey PRIMARY KEY (id);


--
-- Name: item_purchase item_purchase_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.item_purchase
    ADD CONSTRAINT item_purchase_pkey PRIMARY KEY (id_item_purchase);


--
-- Name: product_cost_log product_cost_log_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.product_cost_log
    ADD CONSTRAINT product_cost_log_pkey PRIMARY KEY (log_id);


--
-- Name: product product_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.product
    ADD CONSTRAINT product_pkey PRIMARY KEY (id_product);


--
-- Name: production_purchase production_purchase_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.production_purchase
    ADD CONSTRAINT production_purchase_pkey PRIMARY KEY (id_production_purchase);


--
-- Name: purchase purchase_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.purchase
    ADD CONSTRAINT purchase_pkey PRIMARY KEY (id_purchase);


--
-- Name: role role_pkey; Type: CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.role
    ADD CONSTRAINT role_pkey PRIMARY KEY (id_role);


--
-- Name: item_purchase trg_check_stock_insert; Type: TRIGGER; Schema: art_market_schema; Owner: postgres
--

CREATE TRIGGER trg_check_stock_insert BEFORE INSERT ON art_market_schema.item_purchase FOR EACH ROW EXECUTE FUNCTION art_market_schema.check_stock_on_insert();


--
-- Name: item_purchase trg_deduct_on_accept; Type: TRIGGER; Schema: art_market_schema; Owner: postgres
--

CREATE TRIGGER trg_deduct_on_accept BEFORE UPDATE OF status_item ON art_market_schema.item_purchase FOR EACH ROW WHEN ((((new.status_item)::text = 'accepted'::text) AND ((old.status_item)::text <> 'accepted'::text))) EXECUTE FUNCTION art_market_schema.deduct_stock_on_accept();


--
-- Name: connect_product_material trg_log_cost_on_material_change; Type: TRIGGER; Schema: art_market_schema; Owner: postgres
--

CREATE TRIGGER trg_log_cost_on_material_change AFTER INSERT OR DELETE OR UPDATE ON art_market_schema.connect_product_material FOR EACH ROW EXECUTE FUNCTION art_market_schema.log_product_cost();


--
-- Name: account account_id_role_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.account
    ADD CONSTRAINT account_id_role_fkey FOREIGN KEY (id_role) REFERENCES art_market_schema.role(id_role) ON UPDATE CASCADE;


--
-- Name: connect_product_material connect_product_material_id_material_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.connect_product_material
    ADD CONSTRAINT connect_product_material_id_material_fkey FOREIGN KEY (id_material) REFERENCES art_market_schema.catalog_for_material(id_material) ON UPDATE CASCADE;


--
-- Name: connect_product_material connect_product_material_id_product_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.connect_product_material
    ADD CONSTRAINT connect_product_material_id_product_fkey FOREIGN KEY (id_product) REFERENCES art_market_schema.product(id_product) ON UPDATE CASCADE;


--
-- Name: item_purchase item_purchase_id_product_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.item_purchase
    ADD CONSTRAINT item_purchase_id_product_fkey FOREIGN KEY (id_product) REFERENCES art_market_schema.product(id_product) ON UPDATE CASCADE;


--
-- Name: item_purchase item_purchase_id_purchase_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.item_purchase
    ADD CONSTRAINT item_purchase_id_purchase_fkey FOREIGN KEY (id_purchase) REFERENCES art_market_schema.purchase(id_purchase) ON UPDATE CASCADE;


--
-- Name: product product_id_indiv_buyer_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.product
    ADD CONSTRAINT product_id_indiv_buyer_fkey FOREIGN KEY (id_indiv_buyer) REFERENCES art_market_schema.account(id_account) ON UPDATE CASCADE;


--
-- Name: product product_id_seller_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.product
    ADD CONSTRAINT product_id_seller_fkey FOREIGN KEY (id_seller) REFERENCES art_market_schema.account(id_account) ON UPDATE CASCADE;


--
-- Name: production_purchase production_purchase_id_buyer_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.production_purchase
    ADD CONSTRAINT production_purchase_id_buyer_fkey FOREIGN KEY (id_buyer) REFERENCES art_market_schema.account(id_account) ON UPDATE CASCADE;


--
-- Name: production_purchase production_purchase_id_product_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.production_purchase
    ADD CONSTRAINT production_purchase_id_product_fkey FOREIGN KEY (id_product) REFERENCES art_market_schema.product(id_product) ON UPDATE CASCADE;


--
-- Name: production_purchase production_purchase_id_seller_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.production_purchase
    ADD CONSTRAINT production_purchase_id_seller_fkey FOREIGN KEY (id_seller) REFERENCES art_market_schema.account(id_account) ON UPDATE CASCADE;


--
-- Name: purchase purchase_id_buyer_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.purchase
    ADD CONSTRAINT purchase_id_buyer_fkey FOREIGN KEY (id_buyer) REFERENCES art_market_schema.account(id_account) ON UPDATE CASCADE;


--
-- Name: purchase purchase_id_seller_fkey; Type: FK CONSTRAINT; Schema: art_market_schema; Owner: postgres
--

ALTER TABLE ONLY art_market_schema.purchase
    ADD CONSTRAINT purchase_id_seller_fkey FOREIGN KEY (id_seller) REFERENCES art_market_schema.account(id_account) ON UPDATE CASCADE;


--
-- PostgreSQL database dump complete
--

