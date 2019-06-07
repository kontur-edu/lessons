window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    var $prev = $("#prev_slide_button");
    var $next = $("#next_slide_button");
    var $nextSolutions = $("#next_solutions_button");
    var $nextButtons = $(".next_button");
    
    window.slideNavigation = {
        update: function (hasNext, hasPrev, isAccepted, hideNextSolutions) {
            $next.toggle(hasNext);
            if (!hasNext) {
                $nextButtons.toggle(false);
            }
            $prev.toggle(hasPrev);
            if (!hasPrev) {
                $prev.toggle(false);
            }
            $nextSolutions.toggle(false);
            if (isAccepted && !hideNextSolutions) {
                $nextSolutions.toggle(true);
                $next.toggle(false);
            }
        },
        makeShowSolutionsNext: function () {
            if(!$("#nav_arrows").data("hide")) {
                $next.toggle(false);
                $nextSolutions.toggle(true);
            }
        }
    };
    
    var $parent = $("#nav_arrows");
    window.slideNavigation.update($parent.data("hasnext"), $parent.data("hasprev"), $parent.data("isaccepted"), $parent.data("hide"));
});
