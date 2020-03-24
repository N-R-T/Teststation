function ChangeVisibility(quesNum, resisNum, partNum) {
    var btnName = 'Question' + quesNum + 'CircuitParts' + partNum + 'Resistor' + resisNum + 'VisibilityButton';
    var checkboxName = 'Question' + quesNum + 'CircuitParts' + partNum + 'Resistor' + resisNum + 'Visibility';


    for (var i = 0; i <= 3; i++) {
        $('#Question' + quesNum + 'CircuitParts' + partNum + 'Resistor' + i + 'VisibilityButton')
            .removeClass("glyphicon-eye-close")
            .removeClass("red-color")
            .addClass("glyphicon-eye-open")
            .addClass("green-color");

        $('#Question' + quesNum + 'CircuitParts' + partNum + 'Resistor' + i + 'Visibility')
            .prop("checked", true);
    }

    $("#" + btnName)
        .removeClass("glyphicon-eye-open")
        .removeClass("green-color")
        .addClass("glyphicon-eye-close")
        .addClass("red-color");

    $("#" + checkboxName).prop("checked", false);
}



