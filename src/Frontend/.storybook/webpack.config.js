const path = require("path");

module.exports = (baseConfig, env, defaultConfig) => {

	defaultConfig.module.rules.push(
		{
			test: /\.less$/,
			use: [
				'style-loader',
				{
					loader: 'css-loader',
					options: {
						modules: true,
						localIdentName: '[name]__[local]--[hash:base64:5]',
					},
				},
				'less-loader',
			],
			include: path.resolve(__dirname, '../src/')
		},
	);

	return defaultConfig
};