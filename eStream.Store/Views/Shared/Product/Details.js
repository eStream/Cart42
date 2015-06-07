function ProductDetailsModel(data) {
    var self = this;

    var optionMapping = {
        'options': {
            create: function (option) {
                return (new (function () {
                    this.displayName = ko.computed({
                        read: function () {
                            var name = this.name();
                            /*
                            if (this.priceDifference() != 0)
                                name += ' (' + window.formatCurrency(this.priceDifference()) + ')';
                            */
                            return name;
                        },
                        owner: this
                    });

                    ko.mapping.fromJS(option.data, {}, this);
                })());
            }
        }
    };

    var categoryMapping = {
        'optionCategories': {
            create: function (category) {
                return (new (function () {
                    this.selectedOption = ko.computed({
                        read: function () { },
                        write: function (value) {
                            for (var i = 0; i < this.options().length; i++) {
                                this.options()[i].selected(this.options()[i].id() == value);
                            }
                        },
                        owner: this
                    });

                    ko.mapping.fromJS(category.data, optionMapping, this);
                })());
            }
        }
    };

    ko.mapping.fromJS(data, categoryMapping, self);

    self.getSelectedOptions = function () {
        var selectedOptions = [];
        var categories = self.optionCategories();
        for (var i = 0; i < categories.length; i++) {
            for (var j = 0; j < categories[i].options().length; j++) {
                var option = categories[i].options()[j];
                if (option.selected())
                    selectedOptions.push(option);
            }
        }
        return selectedOptions;
    }

    self.selectedOptions = ko.computed(function () {
        var selectedOptions = self.getSelectedOptions();
        var optionsJson = [];
        for (var i = 0; i < selectedOptions.length; i++) {
            optionsJson.push({
                id: selectedOptions[i].id()
            });
        }
        return JSON.stringify(optionsJson);
    }).extend({ throttle: 100 });

    self.skusFiltered = ko.computed(function () {
        var dummy = self.getSelectedOptions();
        var filteredSkus = [];
        for (var i = 0; i < self.skus().length; i++) {
            var passes = true;
            var sku = self.skus()[i];
            var opts = JSON.parse(sku.optionsJson());

            for (var j = 0; j < opts.length; j++) {
                var opt = opts[j];
                var category = $.grep(self.optionCategories(), function (cat) {
                    return cat.id() == opt.categoryId;
                });

                if (category.length > 0) {
                    category = category[0];
                    var selectedOpts = $.grep(category.options(), function (so) {
                        return so.selected();
                    });
                    if (selectedOpts.length == 0) continue;
                    var matches = $.grep(selectedOpts, function (so) {
                        return so.id() == opt.optionId;
                    });
                    if (matches.length > 0) continue;
                }

                passes = false;
                break;
            }

            if (passes)
                filteredSkus.push(sku);
        }
        return filteredSkus;
    }).extend({ throttle: 100 });

    self.displayPrice = ko.computed(function () {
        var filteredSkus = self.skusFiltered();

        if (filteredSkus.length == 1 && filteredSkus[0].price() > 0) {
            return window.formatCurrency(filteredSkus[0].price());
        }
        if (filteredSkus.length > 1) {
            var minPrice = 0;
            var maxPrice = 0;
            for (var k = 0; k < filteredSkus.length; k++) {
                var testsku = filteredSkus[k];
                var skuPrice = testsku.price();
                if (skuPrice == null) skuPrice = self.salePrice() || self.price();
                if (skuPrice && (minPrice == 0 || skuPrice < minPrice))
                    minPrice = skuPrice;
                if (skuPrice && skuPrice > maxPrice)
                    maxPrice = skuPrice;
            }

            if (minPrice != maxPrice && minPrice != 0) {
                return window.formatCurrency(minPrice) + ' to ' + window.formatCurrency(maxPrice);
            }
            if (minPrice == maxPrice && minPrice != 0) {
                return window.formatCurrency(minPrice);
            }
        }

        return window.formatCurrency(self.salePrice() || self.price());
    }).extend({ throttle: 100 });

    self.displayQty = ko.computed(function() {
        var filteredSkus = self.skusFiltered();

        if (filteredSkus.length == 1 && filteredSkus[0].quantity() != null) {
            return filteredSkus[0].quantity();
        }
        if (filteredSkus.length > 1) {
            return null;
        }

        return self.quantity();
    });

    self.photoIdsFiltered = ko.computed(function () {
        var filteredSkus = self.skusFiltered();
        if (filteredSkus.length == 0 || filteredSkus.length == self.skus().length) return self.photoIds();
        var ids = [];
        for (var i = 0; i < filteredSkus.length; i++) {
            for (var j = 0; j < filteredSkus[i].uploadIds().length; j++) {
                if (ids.indexOf(filteredSkus[i].uploadIds()[j]) < 0)
                    ids.push(filteredSkus[i].uploadIds()[j]);
            }
        }

        if (ids.length == 0)
            return self.photoIdsGeneric();

        return ids;
    }).extend({ throttle: 100 });

    self.photoIdsGeneric = function () {
        var assigned = [];
        for (var i = 0; i < self.skus().length; i++) {
            for (var j = 0; j < self.skus()[i].uploadIds().length; j++) {
                assigned.push(self.skus()[i].uploadIds()[j]);
            }
        }

        return $.grep(self.photoIds(), function (id) {
            return assigned.indexOf(id) < 0;
        });
    }

    self.addToCart = function () {
        var skus = self.skusFiltered();
        if (self.skus().length > 0 && skus.length == 0) {
            bootbox.alert('@T("This combination is not available!")');
            return;
        }
        if (skus.length > 1) {
            bootbox.alert('@T("Please select the product options!")');
            return;
        }
        var qty = 1;
        if ($('#txtAddToCartQty').length)
            qty = $('#txtAddToCartQty').val();
        if (qty == 0) return;

        if (self.displayQty() != null && self.displayQty() < qty) {
            if (qty == 1) {
                bootbox.alert('@T("This product is out of stock!")');
            } else {
                bootbox.alert('@T("The selected quantity is not available!")');
            }
            return;
        }

        $.post('@Url.Action("AddItem", "ShoppingCart")', { productId: self.id(), options: self.selectedOptions(), quantity: qty },
            function done() {
                window.location.href = '@Url.Action("Index", "ShoppingCart")';
            });
    }

    return this;
}