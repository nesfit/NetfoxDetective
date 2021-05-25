//jQuery.noConflict();

jQuery(document).ready(function ($) {

    $('.table > tbody > tr').click(function () {
        console.log('asd');
       

    });

    // UI range slider
    $('.slider_range').each(function () {
        $(this).wrapInner('<div class="slider"/>');
        var data_min = $(this).data('min');
        var data_max = $(this).data('max');
        var data_step = $(this).data('step');
        var data_val = $(this).data('val');
        var data_from = $(this).data('from');
        var data_to = $(this).data('to');
        var input_from = $('[data-input="from"]', this).val();
        var input_to = $('[data-input="to"]', this).val();
        $('.slider', this).slider({
            range: true,
            min: data_min,
            max: data_max,
            step: data_step,
            values: [input_from, input_to],
            slide: function (event, ui) {
                $('[data-input="from"]', this).val(ui.values[0]);
                $('[data-input="to"]', this).val(ui.values[1]);
            },
            stop: function (event, ui) {
                $('[data-input="from"]', this).change();
                $('[data-input="to"]', this).change();
            }
        });
    });
    $('[data-input="from"], [data-input="to"]').change(function () {
        var value = this.value;
        set = 0;
        if ($(this).attr('data-input') == 'to') {
            set = 1;
        }
        el_parent = $(this).parent().parent().parent();
        $('.slider', el_parent).slider("values", set, value);
    });

    $('.mod-filter .btn-reset').click(function () {
        var filters = $(this).parents('.mod-filter');
        $('.slider_range').each(function() {
            $('[data-input="from"]', this).val($(this).data('min'));
            $('[data-input="to"]', this).val($(this).data('max'));
        });
        $('[data-input="from"], [data-input="to"]', filters).change();
    });
   
        
    // Datetime picker

    $('.datetimepicker input').blur(function () {
        $(this).attr('value', $(this).val());
        $(this).change();
    });
    $('.datetimepicker input').focusout(function () {
        $(this).attr('value', $(this).val());
        $(this).change();

    });
    $('.datetimepicker input').change(function () {
        $(this).attr('value', $(this).val());
    });


    // page is loaded
    $('.loading').removeClass('loading').addClass('load');

    dotvvm.events.afterPostback.subscribe(function (args) {
        var o = jQuery('a.image-link').fancybox();
    });


    //Frame Content
    MekeContentTable('.hex');
    MekeContentTable('.ascii');

});

function MekeContentTable(selector) {
    var bytes = $(selector + ' .byte');
    var rows = [];
    while (bytes.length) rows.push(bytes.splice(0, 16));

    var table = $('<div class="hex-table" />');
    var i = 0;
    rows.forEach(function (item, index) {
        var row = $('<div class="row-table" />');
        item.forEach(function (a, b) {
            row.append(a);
        });
        table.append(row);
    });

    $(selector).append(table);
}

/*function loadPage(sender) {
    var href = $(sender).data('href');
    if (typeof href !== "undefined") {
        console.log(href);
        $('#detail .modal-body').load(href, function () {
            $('#detail').modal('show');
        });
    }
}*/

function loadPage(sender) {
    var href = $(sender).data('href');
    var title = $(sender).data('title');
    if (typeof href !== "undefined") {
        $('#detail .modal-header .modal-title').text(title);
        $('#detail .modal-body').height(($(window).height() * 0.9 - 60) + 'px');
        $('#detail .modal-body iframe').remove();
        var ifr = $('<iframe>', {
            src: href,
            id: 'detail-iframe',
            frameborder: 0,
            onload: function () {                
                $('#detail').modal('show');
            }
        }).appendTo('#detail .modal-body');
    }
}