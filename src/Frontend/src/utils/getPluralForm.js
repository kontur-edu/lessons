export default function getPluralForm(count, one, some, many) {
	count %= 100;
	if (count >= 5 && count <= 20) {
		return many;
	}
	count %= 10;
	if (count === 1) {
		return one;
	}
	if (count >= 2 && count < 5) {
		return some;
	}
	return many;
}

