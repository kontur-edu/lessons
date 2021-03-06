process.env.BABEL_ENV = 'development';
process.env.NODE_ENV = 'development';

process.on('unhandledRejection', err => {
	throw err;
});

require('../config/env');

const fs = require('fs-extra');
const chalk = require('chalk');
const webpack = require('webpack');
const webpackDevServer = require('webpack-dev-server');
const clearConsole = require('react-dev-utils/clearConsole');
const checkRequiredFiles = require('react-dev-utils/checkRequiredFiles');
const {
	choosePort,
	createCompiler,
	prepareProxy,
	prepareUrls,
} = require('react-dev-utils/WebpackDevServerUtils');
const openBrowser = require('react-dev-utils/openBrowser');
const ignoredFiles = require('react-dev-utils/ignoredFiles');
const errorOverlayMiddleware = require('react-dev-utils/errorOverlayMiddleware');
const paths = require('../config/paths');
const config = require('../config/webpack.config.dev');


const useYarn = fs.pathExistsSync(paths.yarnLockFile);
const isInteractive = process.stdout.isTTY;

if(!checkRequiredFiles([paths.appHtml, paths.appIndexTsx])) {
	process.exit(1);
}

const DEFAULT_PORT = parseInt(process.env.PORT, 10) || 3000;
const HOST = process.env.HOST || '0.0.0.0';

if(process.env.HOST) {
	console.log(
		chalk.cyan(
			`Attempting to bind to HOST environment variable: ${ chalk.yellow(
				chalk.bold(process.env.HOST)
			) }`
		)
	);
	console.log(
		`If this was unintentional, check that you haven't mistakenly set it in your shell.`
	);
	console.log(`Learn more here: ${ chalk.yellow('http://bit.ly/2mwWSwH') }`);
	console.log();
}

choosePort(HOST, DEFAULT_PORT)
	.then(port => {
		if(port == null) {
			return;
		}
		const protocol = process.env.HTTPS === 'true' ? 'https' : 'http';
		const appName = require(paths.appPackageJson).name;
		const urls = prepareUrls(protocol, HOST, port);
		const proxySetting = require(paths.appPackageJson).proxy;
		const proxyConfig = prepareProxy(proxySetting, paths.appPublic);
		const options = {
			disableHostCheck: !proxyConfig || process.env.DANGEROUSLY_DISABLE_HOST_CHECK === 'true',
			compress: true,
			clientLogLevel: 'info',
			contentBase: paths.appPublic,
			watchContentBase: true,
			hot: true,
			transportMode: 'ws',
			injectClient: false,
			quiet: true,
			watchOptions: {
				ignored: ignoredFiles(paths.appSrc),
			},
			https: protocol === 'https',
			host: HOST,
			overlay: false,
			proxy: proxyConfig,
			historyApiFallback: {
				disableDotRule: true,
			},
			public: urls.lanUrlForConfig,
			publicPath: config.output.publicPath,
			before(app) {
				app.use(errorOverlayMiddleware());
			},
		};
		webpackDevServer.addDevServerEntrypoints(config, options);
		const compiler = createCompiler({
			webpack,
			config,
			appName,
			urls,
			useYarn,
		});
		const devServer = new webpackDevServer(compiler, options);
		devServer.listen(port, HOST, err => {
			if(err) {
				return console.log(err);
			}
			if(isInteractive) {
				clearConsole();
			}
			console.log(chalk.cyan('Starting the development server...\n'));
			openBrowser(urls.localUrlForBrowser);
		});

		['SIGINT', 'SIGTERM'].forEach(function (sig) {
			process.on(sig, function () {
				devServer.close();
				process.exit();
			});
		});
	})
	.catch(err => {
		if(err && err.message) {
			console.log(err);
			console.log(err.message);
		}
		process.exit(1);
	});
