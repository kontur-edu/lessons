import React from "react";
import styles from "./pages.less";

export default function Page({ children }: { children: React.ReactNode }): React.ReactElement {
	return (
		<div className={ styles.wrapper }>
			<div className={ styles.contentWrapper }>
				{ children }
			</div>
		</div>
	);
}
