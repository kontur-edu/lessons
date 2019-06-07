/* This module allows to insert react components inside legacy layout.
 * Use following syntax to render react component:
 * 
 * <div class="react-render" data-component="comment" data-props="@(new { first = "First props", second="Second props"}.JsonSerialize())"></div>
 *  
 * Here `class="react-render"` is a trigger for work,
 * 		`data-component` is a component name (use should define it in Frontend/src/externalComponentRenderer.js), and
 * 		`data-props` are json-serialized props for the component (optional).
 * 	
 * 	Child components are not supported now.
 * 	
 * 	See Frontend/src/externalComponentRenderer.js for the second part of this module.
 */

window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	$('.react-render').each(function () {
		var $this = $(this);
		$this.removeClass('react-render').addClass('react-rendered');
		var componentType = $this.data('component');
		var props = $this.data('props');		
		if (! props)
			props = {};
		
		if (componentType) {
			window.renderReactComponent(componentType, this, props);
		}
	});
});