/**
 * @author      Maxylan
 * @copyright © 2024 Max Olsson
 */

/**
 * City Gross configurations taken straight from citygross.se
 */
const grossconfig = {
    DEBUG: false,
    HOST: 'https://www.citygross.se',
    BASE_URL: /* process.env.API_HOST || */ '/api/v1',
    PICTURE_BASE_URL: '/images/products' /* process.env.PICTURE_BASE_URL || '/images' */,
    CATERING_STORAGE_KEY: 'catering',
    AUTH_STORAGE_KEY: 'cg_reloaded',
    STORAGE_TYPE: 'ls',
    // Doubt these will be needed.
    VERSION: undefined /* __BUILD_VERSION__ */,
    SERVICE_WORKER_ACTIVE: undefined /* false */,
    RECAPTCHA_SITE_KEY: undefined /* process.env.RECAPTCHA_SITE_KEY */,
    USE_RECAPTCHA: undefined /* true */,
}

window.grossconfig = grossconfig;