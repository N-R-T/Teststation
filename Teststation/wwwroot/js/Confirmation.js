function ConfirmDeleteCandidate(name) {
    return ConfirmFunction('Sind Sie sicher, dass Sie den Testkandidat "'+ name +'" löschen wollen?');
};

function ConfirmDeleteTest(name) {
    return ConfirmFunction('Sind Sie sicher, dass Sie den Test "' + name + '" löschen wollen?');
};

function ConfirmReleaseTest(name) {
    return ConfirmFunction('Sind Sie sicher, dass Sie den Test "' + name + '" veröffentlichen wollen? Sie können den Test danach nicht mehr bearbeiten!');
};

function ConfirmFinishTestAnswer() {
    return ConfirmFunction('Sind Sie sicher, dass Sie Ihre Beantwortung beenden wollen? Sie können Ihre Antworten danach nicht mehr verändern!');
};

function ConfirmDeleteSession() {
    return ConfirmFunction('Sind Sie sicher, dass Sie diese fehlerhafte Beantwortung löschen wollen?');
};

function ConfirmFunction(text) {
    if (confirm(text)) {
        return true;
    }
    return false;
}