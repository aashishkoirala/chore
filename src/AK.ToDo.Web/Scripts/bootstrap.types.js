var Bootstrap;
(function (Bootstrap) {
    (function (Modal) {
        var Modes = (function () {
            function Modes() { }
            Modes.show = 'show';
            Modes.hide = 'hide';
            Modes.toggle = 'toggle';
            return Modes;
        })();
        Modal.Modes = Modes;        
        var Events = (function () {
            function Events() { }
            Events.show = 'show';
            Events.shown = 'shown';
            Events.hide = 'hide';
            Events.hidden = 'hidden';
            return Events;
        })();
        Modal.Events = Events;        
    })(Bootstrap.Modal || (Bootstrap.Modal = {}));
    var Modal = Bootstrap.Modal;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (ScrollSpy) {
        var Modes = (function () {
            function Modes() { }
            Modes.refresh = 'refresh';
            return Modes;
        })();
        ScrollSpy.Modes = Modes;        
        var Events = (function () {
            function Events() { }
            Events.activate = 'activate';
            return Events;
        })();
        ScrollSpy.Events = Events;        
    })(Bootstrap.ScrollSpy || (Bootstrap.ScrollSpy = {}));
    var ScrollSpy = Bootstrap.ScrollSpy;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Tab) {
        var Modes = (function () {
            function Modes() { }
            Modes.show = 'show';
            return Modes;
        })();
        Tab.Modes = Modes;        
        var Events = (function () {
            function Events() { }
            Events.show = 'show';
            Events.shown = 'shown';
            return Events;
        })();
        Tab.Events = Events;        
    })(Bootstrap.Tab || (Bootstrap.Tab = {}));
    var Tab = Bootstrap.Tab;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Tooltip) {
        var Modes = (function () {
            function Modes() { }
            Modes.show = 'show';
            Modes.hide = 'hide';
            Modes.toggle = 'toggle';
            Modes.destroy = 'destroy';
            return Modes;
        })();
        Tooltip.Modes = Modes;        
        var Placements = (function () {
            function Placements() { }
            Placements.top = 'top';
            Placements.bottom = 'bottom';
            Placements.left = 'left';
            Placements.right = 'right';
            return Placements;
        })();
        Tooltip.Placements = Placements;        
        var Triggers = (function () {
            function Triggers() { }
            Triggers.click = 'click';
            Triggers.hover = 'hover';
            Triggers.focus = 'focus';
            Triggers.manual = 'manual';
            return Triggers;
        })();
        Tooltip.Triggers = Triggers;        
    })(Bootstrap.Tooltip || (Bootstrap.Tooltip = {}));
    var Tooltip = Bootstrap.Tooltip;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Popover) {
        var Modes = (function () {
            function Modes() { }
            Modes.show = 'show';
            Modes.hide = 'hide';
            Modes.toggle = 'toggle';
            Modes.destroy = 'destroy';
            return Modes;
        })();
        Popover.Modes = Modes;        
        var Placements = (function () {
            function Placements() { }
            Placements.top = 'top';
            Placements.bottom = 'bottom';
            Placements.left = 'left';
            Placements.right = 'right';
            return Placements;
        })();
        Popover.Placements = Placements;        
        var Triggers = (function () {
            function Triggers() { }
            Triggers.click = 'click';
            Triggers.hover = 'hover';
            Triggers.focus = 'focus';
            Triggers.manual = 'manual';
            return Triggers;
        })();
        Popover.Triggers = Triggers;        
    })(Bootstrap.Popover || (Bootstrap.Popover = {}));
    var Popover = Bootstrap.Popover;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Alert) {
        var Modes = (function () {
            function Modes() { }
            Modes.close = 'close';
            return Modes;
        })();
        Alert.Modes = Modes;        
        var Events = (function () {
            function Events() { }
            Events.close = 'close';
            Events.closed = 'closed';
            return Events;
        })();
        Alert.Events = Events;        
    })(Bootstrap.Alert || (Bootstrap.Alert = {}));
    var Alert = Bootstrap.Alert;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Button) {
        var Modes = (function () {
            function Modes() { }
            Modes.toggle = 'toggle';
            Modes.loading = 'loading';
            Modes.reset = 'reset';
            return Modes;
        })();
        Button.Modes = Modes;        
    })(Bootstrap.Button || (Bootstrap.Button = {}));
    var Button = Bootstrap.Button;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Collapse) {
        var Modes = (function () {
            function Modes() { }
            Modes.toggle = 'toggle';
            Modes.show = 'show';
            Modes.hide = 'hide';
            return Modes;
        })();
        Collapse.Modes = Modes;        
        var Events = (function () {
            function Events() { }
            Events.show = 'show';
            Events.shown = 'shown';
            Events.hide = 'hide';
            Events.hidden = 'hidden';
            return Events;
        })();
        Collapse.Events = Events;        
    })(Bootstrap.Collapse || (Bootstrap.Collapse = {}));
    var Collapse = Bootstrap.Collapse;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Carousel) {
        var Modes = (function () {
            function Modes() { }
            Modes.cycle = 'cycle';
            Modes.pause = 'pause';
            Modes.number = 'number';
            Modes.prev = 'prev';
            Modes.next = 'next';
            return Modes;
        })();
        Carousel.Modes = Modes;        
        var Events = (function () {
            function Events() { }
            Events.slide = 'slide';
            Events.slid = 'slid';
            return Events;
        })();
        Carousel.Events = Events;        
    })(Bootstrap.Carousel || (Bootstrap.Carousel = {}));
    var Carousel = Bootstrap.Carousel;
})(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    })(Bootstrap || (Bootstrap = {}));
var Bootstrap;
(function (Bootstrap) {
    (function (Affix) {
        var Modes = (function () {
            function Modes() { }
            Modes.refresh = 'refresh';
            return Modes;
        })();
        Affix.Modes = Modes;        
    })(Bootstrap.Affix || (Bootstrap.Affix = {}));
    var Affix = Bootstrap.Affix;
})(Bootstrap || (Bootstrap = {}));
