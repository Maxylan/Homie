# Platforms are like "instances" of the API.
CREATE TABLE IF NOT EXISTS `platform` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `slug` VARCHAR(63) NOT NULL UNIQUE,
    `name` VARCHAR(63) NOT NULL,
    `master_pswd` VARCHAR(63) NOT NULL,
    `reset_token` VARCHAR(63) NOT NULL,
    PRIMARY KEY (`id`)
);

# Platform-wide settings.
CREATE TABLE IF NOT EXISTS `platform_config` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `key` VARCHAR(63) NOT NULL,
    `value` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`) ON DELETE CASCADE
);

# Platform access tokens.
CREATE TABLE IF NOT EXISTS `platform_tokens` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `name` VARCHAR(63) NOT NULL,
    `token` VARCHAR(63) NOT NULL,
    `expiry` DATETIME NOT NULL,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`) ON DELETE CASCADE,
    INDEX `name_index` (`name`),
    INDEX `token_index` (`token`)
);

# Access log?
CREATE TABLE IF NOT EXISTS `access_log` (
	`id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED DEFAULT null,
    `token` VARCHAR(63) DEFAULT null,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ip` VARCHAR(63) NOT NULL,
    `method` ENUM('GET', 'PUT', 'POST', 'DELETE', 'OPTIONS', 'HEAD', 'PATCH', 'UNKNOWN') NOT NULL DEFAULT 'UNKNOWN',
    `path` VARCHAR(255) NOT NULL,
    `body` TEXT,
    PRIMARY KEY (`id`),
    /* # Intentionally left-out for flexibility, for now.
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`),
    FOREIGN KEY (`token`) REFERENCES platform_tokens(`token`),
    */
    INDEX `ip_index` (`ip`),
    INDEX `method_index` (`method`)
);

# Attachments table
CREATE TABLE IF NOT EXISTS `attachments` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `file` VARCHAR(255) NOT NULL,
    `type` VARCHAR(63) NOT NULL,
    `path` VARCHAR(255),
    `source` VARCHAR(255),
    `alt` VARCHAR(255) NOT NULL DEFAULT '',
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`sender`) REFERENCES platform_tokens(`token`)
);

# Messages table
CREATE TABLE IF NOT EXISTS `messages` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `message` TEXT NOT NULL DEFAULT '',
    `sent` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `sender` VARCHAR(63) NOT NULL,
    `attachment` INT UNSIGNED DEFAULT null,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`) ON DELETE SET null,
    FOREIGN KEY (`sender`) REFERENCES platform_tokens(`token`),
    FOREIGN KEY (`attachment`) REFERENCES attachments(`id`),
    INDEX `sent_index` (`sent`),
    INDEX `sender_index` (`sender`),
    INDEX `message_index` (`message`)
);

# Products table
CREATE TABLE IF NOT EXISTS `products` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`pid` VARCHAR(63) NOT NULL,
    `store` ENUM('citygross', 'ica', 'lidl', 'hemkop') NOT NULL,
    `name` VARCHAR(255) NOT NULL,
    `brand` VARCHAR(63),
    `source` VARCHAR(255),
    `description` TEXT,
    `cover` INT UNSIGNED DEFAULT null,
    `attachment` INT UNSIGNED DEFAULT null,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`cover`) REFERENCES attachments(`id`),
    FOREIGN KEY (`attachment`) REFERENCES attachments(`id`),
    INDEX `pid_index` (`pid`),
    INDEX `store_index` (`store`),
    INDEX `name_index` (`name`)
);

# Product indexes, for grouping products by, and searching/filtering products by `index`.
CREATE TABLE IF NOT EXISTS `product_indexes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`product_id` INT UNSIGNED NOT NULL,
	`pid` VARCHAR(63) NOT NULL,
    `index` ENUM('category', 'super', 'bf', 'tags') NOT NULL,
    `value` VARCHAR(255) NOT NULL,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`product_id`) REFERENCES products(`id`),
    FOREIGN KEY (`pid`) REFERENCES products(`pid`),
    INDEX `value_index` (`index`, `value`)
);

# Product prices.
CREATE TABLE IF NOT EXISTS `product_prices` (
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
    FOREIGN KEY (`product_id`) REFERENCES products(`id`),
    FOREIGN KEY (`pid`) REFERENCES products(`pid`),
    INDEX `price_index` (`current`, `ordinary`),
    INDEX `promotion_index` (`promotion`, `promotion_start`, `promotion_end`)
);