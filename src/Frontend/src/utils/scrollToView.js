export default function scrollToView(element) {
	const header = document.getElementById("header");
	let headerHeight = header.getBoundingClientRect().height;
	const divider = document.getElementById("headerContentDivider");
	const dividerHeight = divider.getBoundingClientRect().height;
	const threadScrollHeight =  element.current.offsetTop - headerHeight - dividerHeight;
	const replyScrollHeight = element.current.getBoundingClientRect().top - element.current.clientHeight + headerHeight + dividerHeight;

	window.scrollTo({
		left: 0,
		top: element.current.offsetParent.localName === "body" ? threadScrollHeight : replyScrollHeight,
		behavior: "smooth",
	});
}