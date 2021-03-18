import React, { useState } from 'react';
import { UnControlled, } from "react-codemirror2";
import { Hint, Toast } from "ui";
import { Copy } from "icons";
import CodeMirror from "./Exercise";

import classNames from "classnames";

import { Language } from "src/consts/languages";
import { Editor, EditorConfiguration } from "codemirror";
import "codemirror/lib/codemirror.css";

import styles from './Exercise.less';
import texts from "src/components/course/Course/Slide/Blocks/Exercise/Exercise.texts";


export interface Props {
	language: Language,
	code: string,
	className?: string,
	hide?: boolean,
	disableStyles?: boolean,
	codeMirrorOptions?: EditorConfiguration,
}

function StaticCode({
	language,
	code,
	className,
	hide,
	codeMirrorOptions,
	disableStyles
}: Props): React.ReactElement<Props> {
	const lines = code.split('\n');
	const [collapseEditor, showAllCode] = useState(hide && lines.length > 20);

	const opts = codeMirrorOptions || {
		lineNumbers: true,
		lineWrapping: true,
		scrollbarStyle: 'null',
		theme: 'default',
		readOnly: true,
		matchBrackets: true,
		extraKeys: {
			"Shift-Tab": false,
			Tab: false
		},
	};

	opts.mode = CodeMirror.loadLanguageStyles(language);
	const value = collapseEditor ? lines.splice(0, 5).join('\n') : code;

	return (
		<div className={ disableStyles ? '' : classNames(styles.wrapper, className) }>
			<UnControlled
				editorDidMount={ onEditorMount }
				className={ disableStyles ? '' : classNames(styles.editor,
					{ [styles.lastLinesFading]: collapseEditor }) }
				options={ opts }
				value={ value }
			/>
			{ collapseEditor &&
			<button className={ styles.showAllCodeButton } onClick={ showAllCodeButtonClicked }>
				{ texts.controls.showAllCode.text }
			</button>
			}
			{ hide && <div className={ styles.copyCodeButton } onClick={ copyCodeButtonClicked }>
				<Hint text={ texts.controls.copyCode.text }>
					<Copy size={ 20 }/>
				</Hint>
			</div>
			}
		</div>
	);

	function onEditorMount(editor: Editor) {
		editor.setSize('auto', '100%');
	}

	function showAllCodeButtonClicked() {
		showAllCode(false);
	}

	function copyCodeButtonClicked() {
		navigator.clipboard.writeText(code);
		Toast.push(texts.controls.copyCode.onCopy);
	}
}

export default StaticCode;
