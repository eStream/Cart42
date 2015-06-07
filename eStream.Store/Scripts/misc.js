Array.prototype.inChunksOf = function(size) {
    var result = [];
    var row = [];
    for (var i = 0; i < this.length; i++) {
        row.push(this[i]);
        if ((i + 1) % size == 0 || i == this.length - 1) {
            result.push(row);
            row = [];
        }
    }
    return result;
}

String.prototype.trunc = String.prototype.trunc ||
    function(n) {
        return this.length > n ? this.substr(0, n - 1) + '...' : this;
    };


$(function () {
    ko.bindingHandlers.bsChecked = {
        init: function (element, valueAccessor, allBindingsAccessor,
            viewModel, bindingContext) {
            var value = valueAccessor();
            var newValueAccessor = function () {
                return {
                    change: function () {
                        value(element.value);
                    }
                }
            };
            ko.bindingHandlers.event.init(element, newValueAccessor,
                allBindingsAccessor, viewModel, bindingContext);
        },
        update: function (element, valueAccessor) {
            if ($(element).val() == ko.unwrap(valueAccessor()) && !$(element).prop('checked')) {
                $(element).closest('.btn').button('toggle');
            }
        }
    }
    $('.partial').each(function (index, item) {
        var url = $(item).data('url');
        if (url && url.length > 0) {
            $(item).load(url);
        }
    });
    $('.disabled').on('click', function () { return false; });

    ko.applyBindings(window.ViewModel);
});