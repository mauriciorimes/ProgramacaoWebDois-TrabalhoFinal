// Maurício Rimes Vieira
// Localização pt-BR para jQuery Validation Plugin
// https://jqueryvalidation.org

(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(["jquery", "jquery-validation"], factory);
    } else if (typeof module === "object" && module.exports) {
        module.exports = factory(require("jquery"));
    } else {
        factory(jQuery);
    }
}(function ($) {

    $.extend($.validator.messages, {
        required: "Este campo é obrigatório.",
        remote: "Por favor, corrija este campo.",
        email: "Por favor, informe um endereço de e-mail válido.",
        url: "Por favor, informe uma URL válida.",
        date: "Por favor, informe uma data válida.",
        dateISO: "Por favor, informe uma data válida (ISO).",
        number: "Por favor, informe um número válido.",
        digits: "Por favor, informe somente dígitos.",
        creditcard: "Por favor, informe um número de cartão de crédito válido.",
        equalTo: "Por favor, informe o mesmo valor novamente.",
        maxlength: $.validator.format("Por favor, informe no máximo {0} caracteres."),
        minlength: $.validator.format("Por favor, informe no mínimo {0} caracteres."),
        rangelength: $.validator.format("Por favor, informe um valor entre {0} e {1} caracteres."),
        range: $.validator.format("Por favor, informe um valor entre {0} e {1}."),
        max: $.validator.format("Por favor, informe um valor menor ou igual a {0}."),
        min: $.validator.format("Por favor, informe um valor maior ou igual a {0}."),
        step: $.validator.format("Por favor, informe um múltiplo de {0}."),
        maxWords: $.validator.format("Por favor, informe no máximo {0} palavras."),
        minWords: $.validator.format("Por favor, informe no mínimo {0} palavras."),
        rangeWords: $.validator.format("Por favor, informe entre {0} e {1} palavras."),
        accept: "Por favor, informe um tipo de arquivo válido.",
        extension: "Por favor, informe um arquivo com extensão válida."
    });

    // Ajuste de DATA para o formato brasileiro (dd/mm/aaaa).
    $.validator.methods.date = function (value, element) {
        var check = false;
        var re = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
        if (re.test(value)) {
            var partes = value.split("/");
            var dia = parseInt(partes[0], 10);
            var mes = parseInt(partes[1], 10);
            var ano = parseInt(partes[2], 10);
            var data = new Date(ano, mes - 1, dia);
            check = (data.getFullYear() === ano) &&
                    (data.getMonth() === mes - 1) &&
                    (data.getDate() === dia);
        }
        return this.optional(element) || check;
    };

    // Ajuste de NÚMERO para o formato brasileiro (ponto de milhar e vírgula decimal).
    $.validator.methods.number = function (value, element) {
        return this.optional(element) ||
            /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value);
    };

}));
