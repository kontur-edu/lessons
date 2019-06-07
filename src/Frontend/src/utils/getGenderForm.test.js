import getGenderForm from './getGenderForm';

describe('getGenderForm', () => {
	test('для мужчины', () => {
		expect(getGenderForm('male', 'была', 'был')).toBe('был');
	});
	test('для женщины', () => {
		expect(getGenderForm('female', 'была', 'был')).toBe('была');
	});
	test('для пустой строки', () => {
		expect(getGenderForm('', 'была', 'был')).toBe('был');
	});
	test('для числа', () => {
		expect(getGenderForm(1, 'была', 'был')).toBe('был');
	});
	test('для числа', () => {
		expect(getGenderForm('1', 'была', 'был')).toBe('был');
	});
	test('для undefined', () => {
		expect(getGenderForm(undefined, 'была', 'был')).toBe('был');
	});
	test('для null', () => {
		expect(getGenderForm(null, 'была', 'был')).toBe('был');
	});
});
