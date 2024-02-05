CREATE TABLE IF NOT EXISTS HomieDB.`access_logs` (
	`id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `platform_id` INT UNSIGNED DEFAULT null,
    `uid` INT UNSIGNED DEFAULT null,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ip` VARCHAR(63) NOT NULL,
    `method` ENUM('GET', 'PUT', 'POST', 'DELETE', 'OPTIONS', 'HEAD', 'PATCH', 'UNKNOWN') NOT NULL DEFAULT 'UNKNOWN',
    `uri` VARCHAR(127) NOT NULL,
    `path` VARCHAR(255) NOT NULL,
    `parameters` VARCHAR(255) NOT NULL,
    `full_url` VARCHAR(511) NOT NULL,
    `headers` VARCHAR(1023) NOT NULL,
    `body` TEXT,
    `response` TEXT,
    `response_status` INT UNSIGNED,
    PRIMARY KEY (`id`),
    /* # Intentionally left-out for flexibility, for now.
    FOREIGN KEY (`platform_id`) REFERENCES platforms(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`uid`) REFERENCES platform_tokens(`id`) ON DELETE SET null,
    */
    INDEX `ip_index` (`ip`),
    INDEX `method_index` (`method`)
);