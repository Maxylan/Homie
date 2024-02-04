# (c) 2024 @Maxylan
CREATE DATABASE IF NOT EXISTS HomieDB;
# USE HomieDB;

# Platforms are like "instances" of the API.
CREATE TABLE IF NOT EXISTS HomieDB.`platforms` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `slug` VARCHAR(63) NOT NULL UNIQUE,
    `code` VARCHAR(63) NOT NULL UNIQUE,
    `name` VARCHAR(63) NOT NULL,
    `master_pswd` VARCHAR(63) NOT NULL,
    `reset_token` VARCHAR(63) NOT NULL,
    PRIMARY KEY (`id`)
);

# Platform-wide settings.
CREATE TABLE IF NOT EXISTS HomieDB.`platform_configs` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `key` VARCHAR(63) NOT NULL,
    `value` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE
);

# Platform access tokens.
CREATE TABLE IF NOT EXISTS HomieDB.`platform_tokens` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `name` VARCHAR(63) NOT NULL,
    `token` VARCHAR(63) NOT NULL,
    `expiry` DATETIME DEFAULT null,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    INDEX `name_index` (`name`),
    INDEX `token_index` (`token`)
);

# Access log?
CREATE TABLE IF NOT EXISTS HomieDB.`access_log` (
	`id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED DEFAULT null,
    `uid` INT UNSIGNED DEFAULT null,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ip` VARCHAR(63) NOT NULL,
    `method` ENUM('GET', 'PUT', 'POST', 'DELETE', 'OPTIONS', 'HEAD', 'PATCH', 'UNKNOWN') NOT NULL DEFAULT 'UNKNOWN',
    `path` TEXT NOT NULL,
    `body` TEXT,
    PRIMARY KEY (`id`),
    /* # Intentionally left-out for flexibility, for now.
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`uid`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    */
    INDEX `ip_index` (`ip`),
    INDEX `method_index` (`method`)
);

# Attachments table
CREATE TABLE IF NOT EXISTS HomieDB.`attachments` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `file` VARCHAR(255) NOT NULL,
    `type` VARCHAR(63) NOT NULL,
    `path` VARCHAR(255),
    `source` VARCHAR(255),
    `alt` VARCHAR(255) NOT NULL DEFAULT '',
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`)
);

# Messages table
CREATE TABLE IF NOT EXISTS HomieDB.`messages` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `message` TEXT NOT NULL,
    `sent` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `sent_by` INT UNSIGNED NOT NULL,
    `attachment` INT UNSIGNED DEFAULT null,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`sent_by`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    FOREIGN KEY (`attachment`) REFERENCES attachments(`id`) ON DELETE SET null,
    INDEX `sent_index` (`sent`),
    INDEX `sender_index` (`sent_by`),
    INDEX `message_index` (`message`)
);

# Products table
CREATE TABLE IF NOT EXISTS HomieDB.`products` (
	`product_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`pid` VARCHAR(63) NOT NULL,
    `store` ENUM('citygross', 'ica', 'lidl', 'hemkop') NOT NULL,
    `name` VARCHAR(255) NOT NULL,
    `brand` VARCHAR(63),
    `source` VARCHAR(255),
    `description` TEXT,
    `cover` INT UNSIGNED DEFAULT null,
    `attachment` INT UNSIGNED DEFAULT null,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`product_id`),
    FOREIGN KEY (`cover`) REFERENCES attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`attachment`) REFERENCES attachments(`id`) ON DELETE SET null,
    INDEX `pid_index` (`pid`),
    INDEX `store_index` (`store`),
    INDEX `name_index` (`name`)
);

# Product indexes, for grouping products by, and searching/filtering products by `index`.
CREATE TABLE IF NOT EXISTS HomieDB.`product_indexes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`product_id` INT UNSIGNED NOT NULL,
	`pid` VARCHAR(63) NOT NULL,
    `index` ENUM('category', 'super', 'bf', 'tags') NOT NULL,
    `value` VARCHAR(255) NOT NULL,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`product_id`) REFERENCES products(`product_id`) ON DELETE CASCADE,
    FOREIGN KEY (`pid`) REFERENCES products(`pid`) ON DELETE CASCADE,
    INDEX `value_index` (`index`, `value`)
);

# Product prices.
CREATE TABLE IF NOT EXISTS HomieDB.`product_prices` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`product_id` INT UNSIGNED NOT NULL,
	`pid` VARCHAR(63) NOT NULL,
    `unit` VARCHAR(63) NOT NULL,
    `current` DECIMAL(10,2) NOT NULL,
    `ordinary` DECIMAL(10,2) NOT NULL,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `promotion` BOOLEAN NOT NULL DEFAULT false,
    `promotion_start` DATETIME,
    `promotion_end` DATETIME,
    `promotion_value` DECIMAL(10,2),
    `promotion_price` DECIMAL(10,2),
    PRIMARY KEY (`id`),
    FOREIGN KEY (`product_id`) REFERENCES products(`product_id`) ON DELETE CASCADE,
    FOREIGN KEY (`pid`) REFERENCES products(`pid`) ON DELETE CASCADE,
    INDEX `price_index` (`current`, `ordinary`),
    INDEX `promotion_index` (`promotion`, `promotion_start`, `promotion_end`)
);

# Shopping Lists!
CREATE TABLE IF NOT EXISTS HomieDB.`lists` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`slug` VARCHAR(255) NOT NULL,
    `platform_id` INT UNSIGNED NOT NULL,
    `title` VARCHAR(255) NOT NULL,
    `created_by` INT UNSIGNED,
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `locked` BOOLEAN NOT NULL DEFAULT false,
    `locked_by` INT UNSIGNED DEFAULT null,
    `cover` INT UNSIGNED DEFAULT null,
    `data` TEXT,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`created_by`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    FOREIGN KEY (`locked_by`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    FOREIGN KEY (`cover`) REFERENCES attachments(`id`) ON DELETE SET null,
    INDEX `created_index` (`created`, `created_by`),
    INDEX `locked_index` (`locked`, `locked_by`),
    INDEX `title_index` (`title`, `slug`, `list_id`)
);

# List Groups
CREATE TABLE IF NOT EXISTS HomieDB.`list_groups` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `list_id` INT UNSIGNED NOT NULL,
    `platform_id` INT UNSIGNED NOT NULL,
    `title` VARCHAR(255) NOT NULL,
    `color` VARCHAR(63) DEFAULT null,
    `cover` INT UNSIGNED DEFAULT null,
    `data` TEXT,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`list_id`) REFERENCES lists(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`cover`) REFERENCES attachments(`id`) ON DELETE SET null,
    INDEX `title_index` (`title`, `slug`, `list_id`)
);

# Notes
CREATE TABLE IF NOT EXISTS HomieDB.`notes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`slug` VARCHAR(255) NOT NULL,
    `platform_id` INT UNSIGNED NOT NULL,
    `title` VARCHAR(255) NOT NULL,
    `color` VARCHAR(63) DEFAULT null,
    `data` TEXT,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    INDEX `title_index` (`title`, `slug`)
);

# Recipes!
CREATE TABLE IF NOT EXISTS HomieDB.`recipes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`slug` VARCHAR(255) NOT NULL,
    `platform_id` INT UNSIGNED NOT NULL,
    `title` VARCHAR(255) NOT NULL,
    `created_by` INT UNSIGNED,
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `locked` BOOLEAN NOT NULL DEFAULT false,
    `locked_by` INT UNSIGNED DEFAULT null,
    `cover` INT UNSIGNED DEFAULT null,
    `data` TEXT,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`created_by`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    FOREIGN KEY (`locked_by`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    FOREIGN KEY (`cover`) REFERENCES attachments(`id`) ON DELETE SET null,
    INDEX `created_index` (`created`, `created_by`),
    INDEX `locked_index` (`locked`, `locked_by`),
    INDEX `title_index` (`title`, `slug`, `list_id`)
);