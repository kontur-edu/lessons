import React, { Component } from "react";
import PropTypes from "prop-types";
import { CopyToClipboard } from 'react-copy-to-clipboard';
import api from "../../../../../api";
import Toggle from "@skbkontur/react-ui/components/Toggle/Toggle";
import Button from "@skbkontur/react-ui/components/Button/Button";
import Toast from "@skbkontur/react-ui/components/Toast/Toast";
import LinkIcon from "@skbkontur/react-icons/Link";
import Input from "@skbkontur/react-ui/components/Input/Input";

import styles from './inviteBlock.less';

class InviteBlock extends Component {
	constructor(props) {
		super(props);
		this.state = {
			isInviteLinkEnabled: props.group.isInviteLinkEnabled
		};
	}

	render() {
		const isInviteLinkEnabled = this.state.isInviteLinkEnabled;

		return (
			<div className={styles["toggle-invite"]}>
				<label>
					<Toggle
						checked={isInviteLinkEnabled}
						onChange={this.onToggle}>
					</Toggle>
					<span className={styles["toggle-invite-text"]}>
						Ссылка для вступления в группу {isInviteLinkEnabled ? ' включена' : ' выключена'}
					</span>
				</label>
				{isInviteLinkEnabled && this.renderInvite()}
			</div>
		)
	}

	renderInvite() {
		const {group} = this.props;

		return (
			<div className={styles["invite-link"]}>
				<div className={styles["invite-link-text"]}>
					<CopyToClipboard
						text={`${window.location.origin}/Account/JoinGroup?hash=${group.inviteHash}`}>
						<Button use="link" icon={<LinkIcon />} onClick={() => Toast.push('Ссылка скопирована')}>
							Скопировать ссылку
						</Button>
					</CopyToClipboard>
				</div>
				<div className={styles["invite-link-input"]}>
					<Input
						type="text"
						value={`${window.location.origin}/Account/JoinGroup?hash=${group.inviteHash}`}
						readOnly
						selectAllOnFocus
						width="65%"
					/>
				</div>
			</div>
		)
	}

	onToggle = () => {
		const {group} = this.props;
		const isInviteLinkEnabled = this.state.isInviteLinkEnabled;

		const update = () => {
			this.setState({
				isInviteLinkEnabled: !isInviteLinkEnabled,
			});
			this.props.group.isInviteLinkEnabled = !isInviteLinkEnabled;
		};
		const revert = () => {
			this.setState({
				isInviteLinkEnabled: isInviteLinkEnabled,
			});
			this.props.group.isInviteLinkEnabled = isInviteLinkEnabled;
		};
		update();

		api.groups.saveGroupSettings(group.id, {'isInviteLinkEnabled': !isInviteLinkEnabled})
		.catch((error) => {
			error.showToast();
			revert();
		});
	};
}

InviteBlock.propTypes = {
	group: PropTypes.object,
};

export default InviteBlock;