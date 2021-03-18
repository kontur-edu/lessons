export enum Gender {
	Male = 'male',
	Female = 'female'
}

export interface ShortUserInfo {
	id: string
	login?: string
	email?: string
	firstName: string
	lastName: string
	visibleName: string
	avatarUrl?: string | null
	gender: Gender | null
}

export interface UsersSearchResponse {
	users: FoundUserResponse[],
}

export interface FoundUserResponse {
	user: ShortUserInfo,
	fields: SearchField[],
}

export enum SearchField {
	UserId,
	Login,
	Name,
	Email,
	SocialLogin,
}
