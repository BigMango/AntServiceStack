/*
 * This is a customized plugin depend on jQuery, jquery.ajaxComboBox.js is the prototype of this one.
 * 
 * we created this just to fit the requirement in our project, we also use the jquer.ajaxComboBox.css as style sheet.
 * 
 *  */

"use strict";

(function ($) {
    $.fn.treeComboBox = function (option) {
        return this.each(function () {
            new TreeComboBox(this, option);
        });
    };

    var TreeComboBox = function (ele, option) {
        this.option = this.setOptions(option);
        this.setProp();
        this.setClass();
        this.setElement(ele);
        this.setBtnAttaPosition();

        this.enButton();
        this.keyupSearch(this);
        this.ehWhole();
    }

    $.extend(TreeComboBox.prototype, {
        setOptions: function (option) {
            return $.extend({
                loadSize: 5,
                field: 'name',
                primary_key: 'id',
                button_img: 'data:image/gif;base64,R0lGODlhFAAUALMAAAB9ui+Vx3++3KjT51+t1J/O5U+lzw+Fvr/e7j+dywR9uv///wAAAAAAAAAAAAAAACH5BAEHAAsALAAAAAAUABQAAARFcMlJq7046837NoCRgSIFnMRFnEB1sEIlsEeFsEAxFThi8SzfjaW7zE6HwesUy6xwKE4CmuggAqyAz/o6bD3gsHhMvkQAADs=',
                bind_to: false,
            }, option);

        },

        setProp: function () {
            this.prop = {
                prev_value: '',
                timer_valchange: false,
                is_suggess: false,
                xhf: false,
                q_changed: true,
                prev_qword: null,
                has_select: false,
            }
        },

        setClass: function () {
            var css_class = {
                container: 'ac_container',
                container_open: 'ac_container_open',
                selected: 'ac_selected',
                re_area: 'ac_result_area',
                navi: 'ac_navi',
                results: 'ac_results',
                re_off: 'ac_results_off',
                select: 'ac_over',
                sub_info: 'ac_subinfo',
                select_ok: 'ac_select_ok',
                select_ng: 'ac_select_ng',
                button: 'ac_button',
                btn_on: 'ac_btn_on',
                btn_out: 'ac_btn_out',
                input: 'ac_input',
                collapsed: 'ac_collapsed',
                collapsible: 'ac_collapsible',
            }

            //			css_class = $.extend(css_class, {input: 'ac_s_input'});
            this.css_class = css_class;

        },

        setElement: function (ele) {
            var elem = {};
            elem.combo_input = $(ele).attr('autocomplete', 'off').addClass(this.css_class.input).wrap('<div>');
            elem.container = $(elem.combo_input).parent().addClass(this.css_class.container);
            elem.clear = $('<div>').css('clear', 'left');
            elem.button = $('<div>').addClass(this.css_class.button);
            elem.img = $('<img>').attr('src', this.option.button_img);

            elem.result_area = $('<div>').addClass(this.css_class.re_area);
            elem.navi = $('<div>').addClass(this.css_class.navi);
            elem.navi_info = $('<div>').addClass('info');
            elem.navi_p = $('<p>');
            elem.results = $('<ul>').addClass(this.css_class.results);
            elem.sub_info = $('<div>').addClass(this.css_class.sub_info);

            var hidden_name = ($(elem.combo_input).attr('name') != undefined) ? $(elem.combo_input).attr('name') : $(elem.combo_input).attr('id');
            hidden_name += '_primary_key';
            elem.hidden = $('<input type="hidden"/>').attr({ name: hidden_name, id: hidden_name }).val('');

            $(elem.container).append(elem.button).append(elem.clear).append(elem.result_area).append(elem.hidden);
            $(elem.button).append(elem.img);

            $(elem.result_area).append(elem.navi).append(elem.results);
            //			.append(elem.sub_info);
            $(elem.navi).append(elem.navi_info);

            $(elem.container).width($(elem.combo_input).outerWidth() + $(elem.button).outerWidth());
            $(elem.button).height($(elem.combo_input).innerHeight());

            this.elem = elem;

        },

        setBtnAttaPosition: function () {
            var width_btn = $(this.elem.button).innerWidth();
            var height_btn = $(this.elem.button).innerHeight();
            var width_img = $(this.elem.img).width();
            var height_img = $(this.elem.img).height();

            var left = width_btn / 2 - (width_img / 2);
            var top = height_btn / 2 - (height_img / 2);

            $(this.elem.img).css({ top: top, left: left });
        },

        enButton: function () {
            var self = this;
            $(self.elem.button).mouseup(function (event) {
                if ($(self.elem.result_area).is(':hidden')) {

                    self._showResultsArea(self);

                } else {

                    self._hideResultsArea(self);

                }
                event.stopPropagation();
            })
			.mouseover(function (event) {
			    $(self.elem.button).addClass(self.css_class.btn_on).removeClass(self.css_class.btn_out);
			})
			.mouseout(function (event) {
			    $(self.elem.button).addClass(self.css_class.btn_out).removeClass(self.css_class.btn_on);
			})
			.mouseout();
        },

        keyupSearch: function () {
            var self = this;
            $(self.elem.combo_input).keyup(function (event) {
                self._abortAjax(self);
                self.prop.has_select = false;
                self._showResultsArea(self);
            });
        },

        _showResultsArea: function (self) {

            if (self.prop.has_select) {
                $(self.elem.result_area).show();
                self.prop.has_select = true;
            } else {
                var q_word = $(self.elem.combo_input).val();
                if (self.prop.prev_qword === q_word) {
                    self.prop.query_changed = false;
                } else {
                    self.prop.prev_qword = q_word;
                    self.prop.query_changed = true;
                }

                if (self.prop.query_changed) {
                    $(self.elem.navi_info).text('loading...')
                    $(self.elem.navi).show();

                    var width = $(self.elem.container).innerWidth();
                    $(self.elem.result_area).css('width', width - 2).show();

                    $(self.elem.results).remove();

                    self._getData(self, q_word);
                } else {
                    $(self.elem.result_area).show();
                }
            }
        },

        _hideResultsArea: function (self) {
            $(self.elem.result_area).hide();
        },

        _getData: function (self, param) {
            var params = { id: param, pageNum: 1, pageSize: self.option.pageSize };
            self.prop.xhr = $.getJSON(self.option.src, params, function (response) {
                var data = response.Data;
                if (data && data.length > 0) {
                    $(self.elem.navi).hide();
                    //debugger;
                    self.elem.results = $('<ul>').addClass(self.css_class.results);

                    var goThroughTree = function (nodes) {
                        $.each(nodes, function (i, item) {
                            //debugger;
                            var ret_ul_li = $('<li>');
                            var li_label = $('<label>').addClass(self.css_class.collapsible).text(item.name).mouseup(function (event) {
                                if ($(this).hasClass(self.css_class.collapsed)) {
                                    $(this).next().hide();
                                    $(this).removeClass(self.css_class.collapsed).addClass(self.css_class.collapsible);
                                } else if ($(this).hasClass(self.css_class.collapsible)) {
                                    $(this).next().show();
                                    $(this).addClass(self.css_class.collapsed).removeClass(self.css_class.collapsible);
                                }
                            });
                            ret_ul_li.append(li_label);

                            var addList = function (list, il) {
                                var ret_ul_li_ul = $('<ul>');
                                $.each(list, function (i, item) {
                                    var chilren = item.list || [];
                                    if (chilren.length) {
                                        var ret_ul_li = $('<li>');
                                        var li_label = $('<label>').addClass(self.css_class.collapsible).text(item.name).mouseup(function (event) {
                                            if ($(this).hasClass(self.css_class.collapsed)) {
                                                $(this).next().hide();
                                                $(this).removeClass(self.css_class.collapsed).addClass(self.css_class.collapsible);
                                            } else if ($(this).hasClass(self.css_class.collapsible)) {
                                                $(this).next().show();
                                                $(this).addClass(self.css_class.collapsed).removeClass(self.css_class.collapsible);
                                            }
                                        });

                                        ret_ul_li.append(li_label);
                                        addList(chilren, ret_ul_li);
                                        console.log(ret_ul_li);
                                        ret_ul_li_ul.append(ret_ul_li);
                                        ret_ul_li_ul.hide();
                                    } else {
                                        //id
                                        var ret_li_ul_li_id = $('<div>').text(item.id);
                                        //name
                                        var ret_li_ul_li_name = $('<div>').text(item.name);
                                        var ret_li_ul_li = $('<li>').append(ret_li_ul_li_id)
                                            .append(ret_li_ul_li_name).mouseover(function () {
                                                $(this).toggleClass(self.css_class.select)
                                            })
                                            .mouseout(function (event) {
                                                $(this).toggleClass(self.css_class.select)
                                            })
                                            .click(function (e) {
                                                e.preventDefault();
                                                e.stopPropagation();
                                                self._selectCurrentLine(self, this);
                                            });
                                        ret_li_ul_li_id.hide();
                                        ret_ul_li_ul.append(ret_li_ul_li);
                                        ret_ul_li_ul.hide();
                                    }



                                })

                                il.append(ret_ul_li_ul);

                            };

                            item.list && addList(item.list, ret_ul_li);
                            $(self.elem.results).append(ret_ul_li);
                            //if (item.more) {
                            //	var ret_more_btn = $('<a>').text('more...').mouseup(function(event) {
                            //		self._loadMore(self, $(event.target).parent(), item.type, param);
                            //	});
                            //	var ret_more_li = $('<li>').append(ret_more_btn);
                            //	ret_ul_li_ul.append(ret_more_li);
                            //}

                        })
                    };

                    goThroughTree(data);
                    $(self.elem.result_area).append(self.elem.results);
                } else {
                    $(self.elem.navi_info).text('no data');
                    $(self.elem.navi).show();
                }
            })
        },

        _calcPageNum: function (self, target) {
            var count = $(target).siblings().length;
            return count / self.option.pageSize + 1;
        },

        _loadMore: function (self, target, type, param) {
            var pageNum = self._calcPageNum(self, target);
            var params = { type: type, id: param, pageNum: pageNum, pageSize: self.option.pageSize };
            $.getJSON(self.option.src, params, function (data) {
                if (data) {
                    var val = data[0];
                    $.each(val.list, function (i, item) {
                        var prepend_li = $('<li>');
                        var prepend_li_id = $('<div>').text(item.id);
                        var prepend_li_name = $('<div>').text(item.name);
                        prepend_li.append(prepend_li_id).append(prepend_li_name).mouseover(function () {
                            $(this).toggleClass(self.css_class.select);
                        })
						.mouseout(function (event) {
						    $(this).toggleClass(self.css_class.select);
						})
						.click(function (e) {
						    e.preventDefault();
						    e.stopPropagation();
						    self._selectCurrentLine(self, this);
						});

                        target.before(prepend_li);
                    });

                    if (!val.more) {
                        target.hide();
                    }
                }
            });
        },

        _abortAjax: function (self) {
            if (self.prop.xhr) {
                self.prop.xhr.abort();
                self.prop.xhr = false;
            }
        },

        ehWhole: function () {
            var self = this;
            var hide_rets = false;
            $(self.elem.container).mousedown(function (event) {
                hide_rets = true;
            });

            $('html').mousedown(function (event) {
                if (hide_rets) {
                    hide_rets = false;
                } else {
                    self._hideResultsArea(self);
                }
            });
        },

        _selectCurrentLine: function (self, target) {
            var current_id = $(target).find("div:first").text();
            var current_val = $(target).find("div:last").text();

            $(self.elem.container).find('li').removeClass(self.css_class.selected);
            $(target).addClass(self.css_class.selected);

            $(self.elem.combo_input).val(current_val);

            if (self.option.bind_to) {
                $(self.elem.combo_input).trigger(self.option.bind_to, { id: current_id, val: current_val });
            }
            self.prop.has_select = true;

            self._hideResultsArea(self);
        }
    });

})(jQuery)