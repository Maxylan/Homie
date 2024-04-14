# (c) 2024 @Maxylan

CREATE DATABASE IF NOT EXISTS HomieDB;
# USE HomieDB;


# Platforms are like "instances" of the API. 
# ("g_" = global, not platform-specific)
CREATE TABLE IF NOT EXISTS HomieDB.`g_platforms` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(63) NOT NULL,
    `guest_code` VARCHAR(31) NOT NULL UNIQUE,
    `member_code` VARCHAR(31) NOT NULL UNIQUE,
    `master_pswd` VARCHAR(63) NOT NULL,
    `reset_token` VARCHAR(63) NOT NULL,
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

# Platform Exports.
# ("g_" = global, not platform-specific)
CREATE TABLE IF NOT EXISTS HomieDB.`g_exports` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `resource_id` INT UNSIGNED NOT NULL COMMENT 'PK Of the exported resource',
    `code` VARCHAR(31) NOT NULL UNIQUE,
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `expires` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE
);

# Platform Users.
CREATE TABLE IF NOT EXISTS HomieDB.`users` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `token` VARCHAR(31) NOT NULL UNIQUE KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `username` VARCHAR(63) NOT NULL,
    `first_name` VARCHAR(63) COMMENT 'optional',
    `last_name` VARCHAR(63) COMMENT 'optional',
    `group` ENUM('banned', 'guest', 'member', 'admin') NOT NULL DEFAULT 'guest',
    `expires` DATETIME null DEFAULT null,
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `last_seen` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE
);

# Platform Settings.
CREATE TABLE IF NOT EXISTS HomieDB.`options` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `key` VARCHAR(63) NOT NULL,
    `value` VARCHAR(255),
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# Access log
# ("g_" = global, not platform-specific)
CREATE TABLE IF NOT EXISTS HomieDB.`g_access_logs` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED null DEFAULT null COMMENT 'platform_id (ON DELETE SET null)',
    `user_token` VARCHAR(31) null DEFAULT null COMMENT 'user_token/token (ON DELETE SET null)',
    `username` VARCHAR(63) null DEFAULT null COMMENT 'username of the requesting user',
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `version` VARCHAR(31),
    `ip` VARCHAR(63) NOT NULL,
    `method` ENUM('GET', 'PUT', 'POST', 'DELETE', 'OPTIONS', 'HEAD', 'PATCH', 'UNKNOWN') NOT NULL DEFAULT 'UNKNOWN',
    `original_url` VARCHAR(1023) NOT NULL,
    `full_url` VARCHAR(1023) NOT NULL,
    `uri` VARCHAR(255) NOT NULL,
    `path` VARCHAR(255),
    `parameters` VARCHAR(511),
    `headers` TEXT,
    `body` TEXT,
    `response_message` VARCHAR(1023) null DEFAULT null,
    `response_status` INT UNSIGNED DEFAULT 503,
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE SET null,
    FOREIGN KEY (`user_token`) REFERENCES HomieDB.users(`token`) ON DELETE SET null
);

# Attachments table
CREATE TABLE IF NOT EXISTS HomieDB.`attachments` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `resource_id` INT UNSIGNED NOT NULL COMMENT 'PK Of the resource this attachment is attached to',
    `file` VARCHAR(255) NOT NULL,
    `type` VARCHAR(127) NOT NULL,
    `alt` VARCHAR(255),
    `blob` MEDIUMBLOB,
    `source` VARCHAR(255),
    `uploaded` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'created',
    `uploaded_by` INT UNSIGNED COMMENT 'author user id (ON DELETE SET null)',
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`uploaded_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# Users Avatars Relationship table.
CREATE TABLE IF NOT EXISTS HomieDB.`user_avatars` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `user_id` INT UNSIGNED NOT NULL COMMENT 'user id (ON DELETE CASCADE)',
    `avatar` INT UNSIGNED NOT NULL COMMENT 'attachment_id (ON DELETE CASCADE)',
    `avatar_sd` INT UNSIGNED COMMENT '(downscaled) attachment_id (ON DELETE SET null)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`user_id`) REFERENCES HomieDB.users(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`avatar`) REFERENCES HomieDB.attachments(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`avatar_sd`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null
);

# Products table
# ("g_" = global, not platform-specific)
CREATE TABLE IF NOT EXISTS HomieDB.`g_products` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`pid` VARCHAR(63) NOT NULL,
    `store` ENUM('citygross', 'ica', 'lidl', 'hemkop') NOT NULL,
    `title` VARCHAR(127) NOT NULL,
    `name` VARCHAR(255) NOT NULL,
    `brand` VARCHAR(63),
    `source` VARCHAR(255),
    `description` TEXT,
    `cover_sd` INT UNSIGNED COMMENT '(downscaled) attachment_id (ON DELETE SET null)' DEFAULT null,
    `has_price` BOOLEAN NOT NULL DEFAULT false,
    `price` DECIMAL(10,2) COMMENT '(has_price)' DEFAULT null,
    `has_discount` BOOLEAN NOT NULL DEFAULT false,
    `discount` DECIMAL(10,2) COMMENT '(has_discount)' DEFAULT null,
    `discount_start` DATETIME COMMENT '(has_discount)' DEFAULT null,
    `discount_end` DATETIME COMMENT '(has_discount)' DEFAULT null,
    `has_amount` BOOLEAN NOT NULL DEFAULT false,
    `amount` DECIMAL(8,4) COMMENT '(has_amount)' DEFAULT null,
    `has_meassurements` BOOLEAN NOT NULL DEFAULT false,
    `meassurements` VARCHAR(63) COMMENT '(has_meassurements)' DEFAULT null,
    `has_weight` BOOLEAN NOT NULL DEFAULT false,
    `weight` DECIMAL(8,4) COMMENT '(has_weight)' DEFAULT null,
    `unit` ENUM('kg', 'hg', 'g') COMMENT '(has_weight)' DEFAULT 'kg',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `changed_by_platform` INT UNSIGNED COMMENT 'platform_id (ON DELETE SET null)',
    FOREIGN KEY (`cover_sd`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by_platform`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE SET null
);

# Product indexes, for grouping products by, and searching/filtering products by `index`.
# ("g_" = global, not platform-specific)
CREATE TABLE IF NOT EXISTS HomieDB.`g_product_indexes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`product_id` INT UNSIGNED NOT NULL,
    `type` ENUM('category', 'super', 'bf', 'tag') NOT NULL,
    `name` VARCHAR(63) NOT NULL,
    `value` VARCHAR(255),
    FOREIGN KEY (`product_id`) REFERENCES HomieDB.g_products(`id`) ON DELETE CASCADE
);

# Visibility table
CREATE TABLE IF NOT EXISTS HomieDB.`visibility` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `resource_id` INT UNSIGNED NOT NULL COMMENT 'PK Of the resource this member does or does not have access to',
    `user_id` INT UNSIGNED NOT NULL COMMENT 'user id (ON DELETE CASCADE)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`user_id`) REFERENCES HomieDB.users(`id`) ON DELETE CASCADE
);

# Notes table
CREATE TABLE IF NOT EXISTS HomieDB.`notes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `visibility` ENUM('private', 'selective', 'inclusive', 'members', 'global') NOT NULL DEFAULT 'global',
    `message` MEDIUMTEXT,
    `color` VARCHAR(63) DEFAULT null,
    `pin` BOOLEAN NOT NULL DEFAULT false,
    `pinned` DATETIME,
    `author` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`author`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# Reminders table
CREATE TABLE IF NOT EXISTS HomieDB.`reminders` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `visibility` ENUM('private', 'selective', 'inclusive', 'members', 'global') NOT NULL DEFAULT 'global',
    `message` VARCHAR(127),
    `deadline` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `has_show_always` BOOLEAN NOT NULL DEFAULT false COMMENT 'If this reminder should always be displayed on the dashboard',
    `has_interval` BOOLEAN NOT NULL DEFAULT false,
    `interval` VARCHAR(63) COMMENT 'If has_interval (repeating) is true, this is the cron expression',
    `has_push_notification` BOOLEAN NOT NULL DEFAULT false,
    `notification_deadline` TIME COMMENT 'If has_push_notification is true, this is the TIME that a notification should be sent (relative to deadline)',
    `has_alarm` BOOLEAN NOT NULL DEFAULT false,
    `has_alarm_vibration` BOOLEAN NOT NULL DEFAULT false COMMENT 'If has_alarm is true, this flags if the alarm should vibrate',
    `has_alarm_sound` BOOLEAN NOT NULL DEFAULT false COMMENT 'If has_alarm is true, this flags if the alarm should be silent',
    `author` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `archive_after` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`author`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# Reminders tags/indexes
CREATE TABLE IF NOT EXISTS HomieDB.`reminder_tags` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `reminder_id` INT UNSIGNED NOT NULL COMMENT 'reminder_id (ON DELETE CASCADE)',
    `name` VARCHAR(63) NOT NULL,
    FOREIGN KEY (`reminder_id`) REFERENCES HomieDB.reminders(`id`) ON DELETE CASCADE
);

# Reminders archive
CREATE TABLE IF NOT EXISTS HomieDB.`reminders_archive` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `visibility` ENUM('private', 'selective', 'inclusive', 'members', 'global') NOT NULL DEFAULT 'global',
    `message` VARCHAR(127),
    `deadline` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `has_show_always` BOOLEAN NOT NULL DEFAULT false COMMENT 'If this reminder should always be displayed on the dashboard',
    `has_interval` BOOLEAN NOT NULL DEFAULT false,
    `interval` VARCHAR(63) COMMENT 'If has_interval (repeating) is true, this is the cron expression',
    `has_push_notification` BOOLEAN NOT NULL DEFAULT false,
    `notification_deadline` TIME COMMENT 'If has_push_notification is true, this is the TIME that a notification should be sent (relative to deadline)',
    `has_alarm` BOOLEAN NOT NULL DEFAULT false,
    `has_alarm_vibration` BOOLEAN NOT NULL DEFAULT false COMMENT 'If has_alarm is true, this flags if the alarm should vibrate',
    `has_alarm_sound` BOOLEAN NOT NULL DEFAULT false COMMENT 'If has_alarm is true, this flags if the alarm should be silent',
    `author` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `archived` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

# Lists! (Like, shopping lists!)
CREATE TABLE IF NOT EXISTS HomieDB.`lists` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `visibility` ENUM('private', 'selective', 'inclusive', 'members', 'global') NOT NULL DEFAULT 'global', /* 
    `type` ENUM('todo', 'shopping', 'ingredients', 'custom') NOT NULL DEFAULT 'custom', */
    `title` VARCHAR(127) NOT NULL,
    `description` VARCHAR(255),
    `cover` INT UNSIGNED COMMENT 'attachment_id (ON DELETE SET null)' DEFAULT null,
    `cover_sd` INT UNSIGNED COMMENT '(downscaled) attachment_id (ON DELETE SET null)' DEFAULT null,
    `author` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`cover`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`cover_sd`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`author`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# List Groups Relationship Table
CREATE TABLE IF NOT EXISTS HomieDB.`groups` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `title` VARCHAR(127) NOT NULL,
    `order` INT UNSIGNED NOT NULL COMMENT 'Ascending' DEFAULT 0,
    `list_id` INT UNSIGNED NOT NULL COMMENT 'list_id (ON DELETE CASCADE)',
    FOREIGN KEY (`list_id`) REFERENCES HomieDB.lists(`id`) ON DELETE CASCADE
);

# List "item" Rows (custom products)
CREATE TABLE IF NOT EXISTS HomieDB.`items` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `list_id` INT UNSIGNED NOT NULL COMMENT 'list_id (ON DELETE CASCADE)',
    `title` VARCHAR(127) NOT NULL,
    `description` TEXT,
    `cover_sd` INT UNSIGNED COMMENT '(downscaled) attachment_id (ON DELETE SET null)' DEFAULT null,
    `has_price` BOOLEAN NOT NULL DEFAULT false,
    `price` DECIMAL(10,2) COMMENT '(has_price)' DEFAULT null,
    `has_discount` BOOLEAN NOT NULL DEFAULT false,
    `discount` DECIMAL(10,2) COMMENT '(has_discount)' DEFAULT null,
    `discount_start` DATETIME COMMENT '(has_discount)' DEFAULT null,
    `discount_end` DATETIME COMMENT '(has_discount)' DEFAULT null,
    `has_amount` BOOLEAN NOT NULL DEFAULT false,
    `amount` DECIMAL(8,4) COMMENT '(has_amount)' DEFAULT null,
    `has_meassurements` BOOLEAN NOT NULL DEFAULT false,
    `meassurements` VARCHAR(63) COMMENT '(has_meassurements)' DEFAULT null,
    `has_weight` BOOLEAN NOT NULL DEFAULT false,
    `weight` DECIMAL(8,4) COMMENT '(has_weight)' DEFAULT null,
    `unit` ENUM('kg', 'hg', 'g') COMMENT '(has_weight)' DEFAULT 'kg',
    `author` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    FOREIGN KEY (`list_id`) REFERENCES HomieDB.lists(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`cover_sd`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`author`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# List Rows
CREATE TABLE IF NOT EXISTS HomieDB.`rows` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `list_id` INT UNSIGNED NOT NULL COMMENT 'list_id (ON DELETE CASCADE)',
    `order` INT UNSIGNED NOT NULL COMMENT 'Ascending' DEFAULT 0,
    `title` VARCHAR(127) NOT NULL DEFAULT '',
    `cover_sd` INT UNSIGNED COMMENT '(downscaled) attachment_id (ON DELETE SET null)' DEFAULT null,
    `has_group` BOOLEAN NOT NULL DEFAULT false,
    `group_id` INT UNSIGNED COMMENT 'group_id (ON DELETE SET null)' DEFAULT null,
    `has_checkbox` BOOLEAN NOT NULL DEFAULT false,
    `store_checkbox` BOOLEAN NOT NULL COMMENT '(has_checkbox)' DEFAULT false,
    `checked` BOOLEAN COMMENT '(has_checkbox)' DEFAULT false,
    `has_timer` BOOLEAN NOT NULL DEFAULT false,
    `deadline` INT UNSIGNED COMMENT '(countdown, seconds)' DEFAULT 30,
    `has_autocomplete` BOOLEAN NOT NULL DEFAULT false,
    `item_id` INT UNSIGNED COMMENT 'item_id (ON DELETE SET null)' DEFAULT null,
    `product_id` INT UNSIGNED COMMENT 'product_id (ON DELETE SET null)' DEFAULT null,
    FOREIGN KEY (`list_id`) REFERENCES HomieDB.lists(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`cover_sd`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`group_id`) REFERENCES HomieDB.`groups`(`id`) ON DELETE SET null,
    FOREIGN KEY (`item_id`) REFERENCES HomieDB.items(`id`) ON DELETE SET null,
    FOREIGN KEY (`product_id`) REFERENCES HomieDB.g_products(`id`) ON DELETE SET null
);

# Recipes!
CREATE TABLE IF NOT EXISTS HomieDB.`recipes` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `platform_id` INT UNSIGNED NOT NULL COMMENT 'platform_id (ON DELETE CASCADE)',
    `visibility` ENUM('private', 'selective', 'inclusive', 'members', 'global') NOT NULL DEFAULT 'global',
    `title` VARCHAR(127) NOT NULL,
    `description` VARCHAR(255),
    `cooking_time` TIME COMMENT 'This is the TIME to COOK',
    `cover` INT UNSIGNED COMMENT 'attachment_id (ON DELETE SET null)' DEFAULT null,
    `cover_sd` INT UNSIGNED COMMENT '(downscaled) attachment_id (ON DELETE SET null)' DEFAULT null,
    `portions_from` INT UNSIGNED DEFAULT 2,
    `portions_to` INT UNSIGNED DEFAULT 4,
    `ingredients_list_id` INT UNSIGNED COMMENT 'list_id (ON DELETE SET null)',
    `todo_list_id` INT UNSIGNED COMMENT 'list_id (ON DELETE SET null)',
    `author` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `changed_by` INT UNSIGNED COMMENT 'user id (ON DELETE SET null)',
    FOREIGN KEY (`platform_id`) REFERENCES HomieDB.g_platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`cover`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`cover_sd`) REFERENCES HomieDB.attachments(`id`) ON DELETE SET null,
    FOREIGN KEY (`ingredients_list_id`) REFERENCES HomieDB.lists(`id`) ON DELETE SET null,
    FOREIGN KEY (`todo_list_id`) REFERENCES HomieDB.lists(`id`) ON DELETE SET null,
    FOREIGN KEY (`author`) REFERENCES HomieDB.users(`id`) ON DELETE SET null,
    FOREIGN KEY (`changed_by`) REFERENCES HomieDB.users(`id`) ON DELETE SET null
);

# Recipes tags/indexes
CREATE TABLE IF NOT EXISTS HomieDB.`recipe_tags` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `recipe_id` INT UNSIGNED NOT NULL COMMENT 'recipe_id (ON DELETE CASCADE)',
    `name` VARCHAR(63) NOT NULL,
    FOREIGN KEY (`recipe_id`) REFERENCES HomieDB.recipes(`id`) ON DELETE CASCADE
);

# Recipe ratings
CREATE TABLE IF NOT EXISTS HomieDB.`recipe_ratings` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `recipe_id` INT UNSIGNED NOT NULL COMMENT 'recipe_id (ON DELETE CASCADE)',
    `user_id` INT UNSIGNED NOT NULL COMMENT 'user id (ON DELETE CASCADE)',
    `rating` INT UNSIGNED NOT NULL COMMENT '0-10' DEFAULT 5,
    FOREIGN KEY (`recipe_id`) REFERENCES HomieDB.recipes(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`user_id`) REFERENCES HomieDB.users(`id`) ON DELETE CASCADE
);

# Recipe favorites
CREATE TABLE IF NOT EXISTS HomieDB.`recipe_favorites` (
    `recipe_id` INT UNSIGNED NOT NULL COMMENT 'recipe_id (ON DELETE CASCADE)',
    `user_id` INT UNSIGNED NOT NULL COMMENT 'user id (ON DELETE CASCADE)',
    FOREIGN KEY (`recipe_id`) REFERENCES HomieDB.recipes(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`user_id`) REFERENCES HomieDB.users(`id`) ON DELETE CASCADE,
    PRIMARY KEY (`recipe_id`, `user_id`)
);