$(document).ready(function () {

    $('#side-menu').metisMenu();

    $('.collapse-link').click(function () {
        var ibox = $(this).closest('div.ibox');
        var button = $(this).find('i');
        var content = ibox.find('div.ibox-content');
        content.slideToggle(200);
        button.toggleClass('fa-chevron-up').toggleClass('fa-chevron-down');
        ibox.toggleClass('').toggleClass('border-bottom');
        setTimeout(function () {
            ibox.resize();
            ibox.find('[id^=map-]').resize();
        }, 50);
    });

    $('.close-link').click(function () {
        var content = $(this).closest('div.ibox');
        content.remove();
    });

    $('.check-link').click(function () {
        var button = $(this).find('i');
        var label = $(this).next('span');
        button.toggleClass('fa-check-square').toggleClass('fa-square-o');
        label.toggleClass('todo-completed');
        return false;
    });

    $('.navbar-minimalize').click(function () {
        $("body").toggleClass("mini-navbar");
        SmoothlyMenu();
    });

    $('.tooltip-demo').tooltip({
        selector: "[data-toggle=tooltip]",
        container: "body"
    });

    function fixHeight() {
        var heightWithoutNavbar = $("body > #wrapper").height() - 61;
        $(".sidebard-panel").css("min-height", heightWithoutNavbar + "px");
    }
    fixHeight();

    $(window).bind("load resize click scroll", function () {
        if (!$("body").hasClass('body-small')) {
            fixHeight();
        }
    });

    $("[data-toggle=popover]")
        .popover();
});

function animationHover(element, animation) {
    element = $(element);
    element.hover(
        function () {
            element.addClass('animated ' + animation);
        },
        function () {
            //wait for animation to finish before removing classes
            window.setTimeout(function () {
                element.removeClass('animated ' + animation);
            }, 2000);
        });
}

$(function () {
    $(window).bind("load resize", function () {
        if ($(this).width() < 769) {
            $('body').addClass('body-small');
        } else {
            $('body').removeClass('body-small');
        }
    });
});

function SmoothlyMenu() {
    if (!$('body').hasClass('mini-navbar') || $('body').hasClass('body-small')) {
        $('#side-menu').hide();
        setTimeout(
            function () {
                $('#side-menu').fadeIn(500);
            }, 100);
    } else {
        $('#side-menu').removeAttr('style');
    }
}

function WinMove() {
    $("div.ibox").not('.no-drop')
        .draggable({
            revert: true,
            zIndex: 2000,
            cursor: "move",
            handle: '.ibox-title',
            opacity: 0.8,
            drag: function () {
                var finalOffset = $(this).offset();
                var finalxPos = finalOffset.left;
                var finalyPos = finalOffset.top;
                // Add div with above id to see position of panel
                $('#posX').text('Final X: ' + finalxPos);
                $('#posY').text('Final Y: ' + finalyPos);
            },
        })
        .droppable({
            tolerance: 'pointer',
            drop: function (event, ui) {
                var draggable = ui.draggable;
                var droppable = $(this);
                var dragPos = draggable.position();
                var dropPos = droppable.position();
                draggable.swap(droppable);
                setTimeout(function () {
                    var dropmap = droppable.find('[id^=map-]');
                    var dragmap = draggable.find('[id^=map-]');
                    if (dragmap.length > 0 || dropmap.length > 0) {
                        dragmap.resize();
                        dropmap.resize();
                    }
                    else {
                        draggable.resize();
                        droppable.resize();
                    }
                }, 50);
                setTimeout(function () {
                    draggable.find('[id^=map-]').resize();
                    droppable.find('[id^=map-]').resize();
                }, 250);
            }
        });
}
jQuery.fn.swap = function (b) {
    b = jQuery(b)[0];
    var a = this[0];
    var t = a.parentNode.insertBefore(document.createTextNode(''), a);
    b.parentNode.insertBefore(a, b);
    t.parentNode.insertBefore(b, t);
    t.parentNode.removeChild(t);
    return this;
};
