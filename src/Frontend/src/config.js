let config = window.config;

/* By default configuration is provided by backend via inserting JSON in index.html. If backend didn't provide
 * configuration (i.e. in local environment for launches via webpack dev server), load it from settings.json */
if (!config || Object.keys(config).length === 0) {
	config = require("./settings.json");

	/* Replace api.endpoint to be ready for production environment */
	const hostname = window.location.hostname;
	if (hostname !== 'localhost' && hostname !== '127.0.0.1') {
		/* Just add "api." to hostname, i.e. ulearn.me → api.ulearn.me */
		config.api.endpoint = window.location.protocol + '//api.' + hostname + '/';
	}
}

export default config;