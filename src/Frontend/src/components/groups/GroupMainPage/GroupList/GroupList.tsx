import React from "react";
import { Loader } from "ui";
import GroupInfo from "../GroupInfo/GroupInfo";

import { GroupInfo as GroupInfoType } from "src/models/groups";

import styles from "./groupList.less";

interface Props {
	courseId: string;
	groups: GroupInfoType[];
	loading: boolean;

	userId?: string | null;

	children?: React.ReactNode;

	deleteGroup: (group: GroupInfoType, groupType: 'archiveGroups' | 'groups') => void;
	toggleArchived: (group: GroupInfoType, isNotArchived: boolean) => void;
}

function GroupList({
	courseId,
	groups,
	loading,
	deleteGroup,
	toggleArchived,
	children,
	userId,
}: Props): React.ReactElement {
	return (
		<section className={ styles.wrapper }>
			{ loading &&
			<div className={ styles.loaderWrapper }>
				<Loader type="big" active={ true }/>
			</div>
			}
			{ !loading &&
			<div className={ styles.content }>
				{ groups && (JSON.parse(JSON.stringify(groups)) as GroupInfoType[])
					.sort((a, b) => {
						if(userId) {
							const teachersInA = new Set([a.owner.id, ...a.accesses.map(item => item.user.id)]);
							const isUserInA = teachersInA.has(userId);
							const teachersInB = new Set([b.owner.id, ...b.accesses.map(item => item.user.id)]);
							const isUserInB = teachersInB.has(userId);

							if(teachersInA.size === 1 && isUserInA && teachersInB.size === 1 && isUserInB) {
								return 0;
							}

							if(teachersInA.size === 1 && isUserInA) {
								return -1;
							}

							if(teachersInB.size === 1 && isUserInB) {
								return 1;
							}

							if(isUserInA && isUserInB) {
								return 0;
							}
							if(isUserInA) {
								return -1;
							}
							if(isUserInB) {
								return 1;
							}
						}

						return a.name.localeCompare(b.name);
					})
					.map(group =>
						<GroupInfo
							key={ group.id }
							courseId={ courseId }
							group={ group }
							deleteGroup={ deleteGroup }
							toggleArchived={ toggleArchived }
						/>)
				}
			</div>
			}
			{ !loading && groups && groups.length === 0 &&
			<div className={ styles.noGroups }>
				{ children }
			</div>
			}
		</section>
	);
}

export default GroupList;
