# CREATE DATABASE IF NOT EXISTS HomieDB;
# CREATE USER 'homie'@'%' IDENTIFIED WITH mysql_native_password BY 'stronger-password';
# GRANT CREATE, ALTER, INSERT, UPDATE, DELETE, SELECT, REFERENCES ON HomieDB.* TO 'homie'@'%';
# CREATE USER 'proxy'@'%' IDENTIFIED WITH mysql_native_password BY 'password';
# GRANT INSERT, UPDATE, SELECT ON HomieDB.* TO 'proxy'@'%';