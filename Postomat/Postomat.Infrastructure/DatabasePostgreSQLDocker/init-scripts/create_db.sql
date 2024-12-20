SELECT 'CREATE DATABASE "PostomatDB" OWNER postgres'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'PostomatDB')
LIMIT 1;
CREATE DATABASE "PostomatDB" OWNER postgres;