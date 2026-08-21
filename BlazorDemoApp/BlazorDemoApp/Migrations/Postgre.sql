-- ============================================================================
-- POSTGRESQL ADVANCED DEMO SCHEMA
-- Autor: Mateusz Dłutek
-- Opis: Kompleksowa struktura bazy: Relacje, JSONB, Tablice, Procedury, Triggery
-- ============================================================================

-- 0. Czyszczenie środowiska (opcjonalne przy resecie)
DROP TABLE IF EXISTS audit_logs CASCADE;
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS products CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS customers CASCADE;
DROP TYPE IF EXISTS order_status CASCADE;

-- 1. Definicja typu wyliczeniowego (ENUM)
CREATE TYPE order_status AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'cancelled');

-- 2. Tabela Klientów (Użycie UUID, Tablic TEXT[] i JSONB)
CREATE TABLE customers (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_uid UUID DEFAULT gen_random_uuid() UNIQUE NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    phone_numbers TEXT[] DEFAULT '{}', -- Tablica numerów telefonów
    preferences JSONB DEFAULT '{"theme": "dark", "newsletter": true}'::jsonb,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 3. Tabela Kategorii
CREATE TABLE categories (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL
);

-- 4. Tabela Produktów (Relacja FK, Tablice, JSONB ze specyfikacją)
CREATE TABLE products (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    category_id INT NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    sku VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(200) NOT NULL,
    price NUMERIC(10, 2) NOT NULL CHECK (price >= 0),
    stock_quantity INT NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    tags TEXT[] DEFAULT '{}', -- Tablica tagów produktu
    specifications JSONB DEFAULT '{}'::jsonb, -- Np. {"ram": "16GB", "cpu": "M3"}
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Indeks GIN na kolumnie JSONB (zapewnia ultraszybkie przeszukiwanie właściwości JSON)
CREATE INDEX idx_products_specs_gin ON products USING GIN (specifications);

-- 5. Tabela Zamówień (Relacja do klienta, ENUM, JSONB dla adresu)
CREATE TABLE orders (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_number UUID DEFAULT gen_random_uuid() UNIQUE NOT NULL,
    customer_id INT NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    status order_status DEFAULT 'pending' NOT NULL,
    shipping_address JSONB NOT NULL, -- Np. {"city": "Warszawa", "street": "Złota 44", "zip": "00-120"}
    total_amount NUMERIC(12, 2) DEFAULT 0.00 NOT NULL,
    order_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 6. Tabela Pozycji Zamówienia (Relacje N:M, Kolumna Generowana / Computed)
CREATE TABLE order_items (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id INT NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
    unit_price NUMERIC(10, 2) NOT NULL CHECK (unit_price >= 0),
    quantity INT NOT NULL CHECK (quantity > 0),
    -- Kolumna automatycznie wyliczana (STORED w bazie Postgresa)
    line_total NUMERIC(12, 2) GENERATED ALWAYS AS (unit_price * quantity) STORED
);

-- 7. Tabela Audytu (Dla Triggera)
CREATE TABLE audit_logs (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    table_name VARCHAR(50) NOT NULL,
    operation VARCHAR(10) NOT NULL,
    record_id INT,
    old_data JSONB,
    new_data JSONB,
    performed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- ============================================================================
-- TRIGGER: Automatyczny audyt zmian w tabeli produktów (zapis do JSONB)
-- ============================================================================
CREATE OR REPLACE FUNCTION fn_audit_products_change()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'UPDATE') THEN
        INSERT INTO audit_logs (table_name, operation, record_id, old_data, new_data)
        VALUES ('products', 'UPDATE', OLD.id, to_jsonb(OLD), to_jsonb(NEW));
        RETURN NEW;
    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO audit_logs (table_name, operation, record_id, old_data, new_data)
        VALUES ('products', 'DELETE', OLD.id, to_jsonb(OLD), NULL);
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_products_audit
AFTER UPDATE OR DELETE ON products
FOR EACH ROW
EXECUTE FUNCTION fn_audit_products_change();

-- ============================================================================
-- FUNKCJA (Function): Oblicza łączną wartość wydanych pieniędzy przez klienta
-- ============================================================================
CREATE OR REPLACE FUNCTION fn_get_customer_total_spent(p_customer_id INT)
RETURNS NUMERIC(12,2) AS $$
DECLARE
    v_total NUMERIC(12,2);
BEGIN
    SELECT COALESCE(SUM(total_amount), 0.00)
    INTO v_total
    FROM orders
    WHERE customer_id = p_customer_id AND status != 'cancelled';
    
    RETURN v_total;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- PROCEDURA (Procedure): Tworzy zamówienie, zdejmuje stan magazynowy i liczy sumę
-- ============================================================================
CREATE OR REPLACE PROCEDURE sp_place_simple_order(
    p_customer_id INT,
    p_product_id INT,
    p_quantity INT,
    p_shipping_address JSONB
)
AS $$
DECLARE
    v_product_price NUMERIC(10,2);
    v_available_stock INT;
    v_new_order_id BIGINT;
BEGIN
    -- 1. Sprawdź stan magazynowy i cenę
    SELECT price, stock_quantity INTO v_product_price, v_available_stock
    FROM products WHERE id = p_product_id FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Produkt o ID % nie istnieje!', p_product_id;
    END IF;

    IF v_available_stock < p_quantity THEN
        RAISE EXCEPTION 'Brak wystarczającej ilości w magazynie. Dostępne: %, Żądano: %', v_available_stock, p_quantity;
    END IF;

    -- 2. Utwórz zamówienie
    INSERT INTO orders (customer_id, status, shipping_address, total_amount)
    VALUES (p_customer_id, 'processing', p_shipping_address, (v_product_price * p_quantity))
    RETURNING id INTO v_new_order_id;

    -- 3. Utwórz pozycję zamówienia
    INSERT INTO order_items (order_id, product_id, unit_price, quantity)
    VALUES (v_new_order_id, p_product_id, v_product_price, p_quantity);

    -- 4. Odejmij stan magazynowy
    UPDATE products
    SET stock_quantity = stock_quantity - p_quantity
    WHERE id = p_product_id;

    RAISE NOTICE 'Zamówienie #% utworzone pomyślnie na kwotę % PLN.', v_new_order_id, (v_product_price * p_quantity);
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- PRZYKŁADOWE DANE STARTOWE (SEED DATA)
-- ============================================================================

-- Klienci
INSERT INTO customers (first_name, last_name, email, phone_numbers, preferences) VALUES
('Jan', 'Kowalski', 'jan.kowalski@example.com', ARRAY['+48123456789', '+48987654321'], '{"newsletter": true, "vip": true, "theme": "dark"}'::jsonb),
('Anna', 'Nowak', 'anna.nowak@example.com', ARRAY['+48500600700'], '{"newsletter": false, "vip": false, "theme": "light"}'::jsonb),
('Piotr', 'Wiśniewski', 'piotr.w@example.com', '{}', '{"newsletter": true, "vip": false}'::jsonb);

-- Kategorie
INSERT INTO categories (name, slug) VALUES
('Elektronika', 'elektronika'),
('Książki & Edukacja', 'ksiazki-edukacja'),
('Akcesoria Biurowe', 'akcesoria-biurowe');

-- Produkty (z tablicami tagów i obiektami JSONB)
INSERT INTO products (category_id, sku, name, price, stock_quantity, tags, specifications) VALUES
(1, 'LAP-DELL-XPS', 'Laptop Dell XPS 15', 7499.00, 15, ARRAY['laptop', 'komputer', 'dell', 'premium'], '{"cpu": "i7-13700H", "ram_gb": 32, "weight_kg": 1.86, "display": "OLED 3.5K"}'::jsonb),
(1, 'MOU-LOGI-MX', 'Mysz Logitech MX Master 3S', 459.00, 40, ARRAY['mysz', 'akcesoria', 'logitech', 'bluetooth'], '{"dpi": 8000, "connectivity": "Bluetooth/USB", "color": "Graphite"}'::jsonb),
(2, 'BOOK-DOTNET-01', 'C# 12 and .NET 8 Guide', 179.00, 100, ARRAY['ksiazka', 'programowanie', 'dotnet', 'csharp'], '{"pages": 820, "cover": "Hardcover", "edition": 2024}'::jsonb),
(3, 'DSK-ERG-STAND', 'Ergonomiczna podstawka pod laptopa', 129.00, 25, ARRAY['akcesoria', 'ergonomia', 'biurko'], '{"material": "Aluminium", "max_load_kg": 10}'::jsonb);

-- Wywołanie procedury złożenia zamówienia
CALL sp_place_simple_order(
    1, 
    1, 
    1, 
    '{"city": "Warszawa", "street": "Marszałkowska 100", "zip": "00-001"}'::jsonb
);

CALL sp_place_simple_order(
    2, 
    2, 
    2, 
    '{"city": "Kraków", "street": "Floriańska 15", "zip": "31-019"}'::jsonb
);

-- Test triggera audytu (zmiana ceny)
UPDATE products SET price = 7199.00, stock_quantity = 20 WHERE sku = 'LAP-DELL-XPS';

return;

-- 1. Zaawansowany JOIN z agregacją i statusem
SELECT 
    c.id AS customer_id,
    c.first_name || ' ' || c.last_name AS customer_name,
    COUNT(o.id) AS total_orders,
    COALESCE(SUM(o.total_amount), 0) AS total_spent,
    fn_get_customer_total_spent(c.id) AS function_calculated_spent
FROM customers c
LEFT JOIN orders o ON c.id = o.customer_id
GROUP BY c.id, c.first_name, c.last_name
ORDER BY total_spent DESC;

-- 2. Przeszukiwanie wnętrza kolumny JSONB (Właściwości JSON)


-- Pobierz produkty, które mają więcej niż 16 GB RAM w specyfikacji JSONB:
SELECT 
    name, 
    price, 
    specifications ->> 'cpu' AS processor,
    (specifications ->> 'ram_gb')::INT AS ram_size
FROM products
WHERE (specifications ->> 'ram_gb')::INT >= 16;

-- 3. Przeszukiwanie tablicy Postgresa (TEXT[])
-- Znajdź produkty, które zawierają tag 'akcesoria' w tablicy:
SELECT name, price, tags
FROM products
WHERE 'akcesoria' = ANY(tags);

-- 4. Podgląd logów audytu utworzonych przez Trigger

-- Zobacz, co zarejestrował nasz trigger podczas UPDATE na produkcie:
SELECT 
    operation,
    performed_at,
    old_data ->> 'price' AS old_price,
    new_data ->> 'price' AS new_price,
    new_data ->> 'name' AS product_name
FROM audit_logs;

-- 5. Funkcja okna (Window Function) – Ranking najdroższych produktów w kategorii

SELECT 
    p.name,
    cat.name AS category,
    p.price,
    DENSE_RANK() OVER (PARTITION BY p.category_id ORDER BY p.price DESC) AS price_rank
FROM products p
JOIN categories cat ON p.category_id = cat.id;