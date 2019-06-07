export default function getGenderForm(gender, female, male) {
	if (gender === 'female') {
		return female;
	} else {
		return male;
	}
}