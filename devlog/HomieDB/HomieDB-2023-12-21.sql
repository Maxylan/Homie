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
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`)
);

# Platform-wide settings.
CREATE TABLE IF NOT EXISTS `platform_tokens` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `name` VARCHAR(63) NOT NULL,
    `token` VARCHAR(63) NOT NULL,
    `expiry` DATETIME NOT NULL,
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`)
);

# Access log?
CREATE TABLE IF NOT EXISTS `access_log` (
	`id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED DEFAULT null,
    `token` INT UNSIGNED DEFAULT null,
    `datetime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ip` VARCHAR(63) NOT NULL,
    `method` ENUM('GET', 'PUT', 'POST', 'DELETE', 'OPTIONS', 'HEAD', 'PATCH') NOT NULL,
    `path` VARCHAR(255) NOT NULL,
    `body` TEXT,
    PRIMARY KEY (`id`)
);

# 
CREATE TABLE IF NOT EXISTS `message` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED NOT NULL,
    `message` TEXT NOT NULL DEFAULT '',
    `sent` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `sender` VARCHAR(63) NOT NULL 
    PRIMARY KEY (`id`),
    FOREIGN KEY (`platform_id`) REFERENCES platform(`id`),
    FOREIGN KEY (`sender`) REFERENCES platform_tokens(`token`)
);