-- Auto-create all service databases on first start
SELECT 'CREATE DATABASE orders_db'    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'orders_db')    \gexec
SELECT 'CREATE DATABASE customers_db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'customers_db') \gexec
SELECT 'CREATE DATABASE payments_db'  WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'payments_db')  \gexec
SELECT 'CREATE DATABASE inventory_db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'inventory_db') \gexec
