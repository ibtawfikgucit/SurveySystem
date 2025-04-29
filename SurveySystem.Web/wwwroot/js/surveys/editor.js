// This JavaScript file contains functions for the survey editor

// Initialize the survey editor
function initSurveyEditor() {
    // Show/hide options based on question type
    $('#questionType').on('change', function () {
        updateOptionVisibility($(this).val());
    });

    // Add option
    $('#addOptionBtn').on('click', function () {
        addOption();
    });

    // Remove option
    $(document).on('click', '.remove-option-btn', function () {
        $(this).closest('.option-item').remove();
    });

    // Save question
    $('#saveQuestionBtn').on('click', function () {
        saveQuestion();
    });

    // Update question
    $('#updateQuestionBtn').on('click', function () {
        updateQuestion();
    });

    // Edit question
    $(document).on('click', '.edit-question-btn', function () {
        editQuestion($(this).data('id'));
    });

    // Delete question
    $(document).on('click', '.delete-question-btn', function () {
        if (confirm('Are you sure you want to delete this question?')) {
            deleteQuestion($(this).data('id'));
        }
    });

    // Copy survey link
    $('#copySurveyLink').on('click', function (e) {
        e.preventDefault();
        copySurveyLink();
    });

    // Initialize sortable questions
    if (document.getElementById('questionList')) {
        new Sortable(document.getElementById('questionList'), {
            animation: 150,
            ghostClass: 'sortable-ghost',
            onEnd: function () {
                updateQuestionOrder();
            }
        });
    }
}

// Update option visibility based on question type
function updateOptionVisibility(questionType) {
    if (questionType === 'SingleChoice' || questionType === 'MultipleChoice') {
        $('#optionsContainer').removeClass('d-none');
        if ($('#optionsList .option-item').length === 0) {
            addOption();
            addOption();
        }
    } else {
        $('#optionsContainer').addClass('d-none');
    }
}

// Add an option to the options list
function addOption() {
    const optionHtml = `
        <div class="input-group mb-2 option-item">
            <input type="text" class="form-control option-text" placeholder="Option text" required>
            <button class="btn btn-outline-danger remove-option-btn" type="button">
                <i class="bi bi-x"></i>
            </button>
        </div>
    `;
    $('#optionsList').append(optionHtml);
}

// Save a new question
function saveQuestion() {
    const surveyId = $('#surveyId').val();

    const question = {
        text: $('#questionText').val(),
        description: $('#questionDescription').val(),
        type: $('#questionType').val(),
        isRequired: $('#isRequired').is(':checked'),
        options: []
    };

    if (question.type === 'SingleChoice' || question.type === 'MultipleChoice') {
        $('.option-text').each(function (index) {
            question.options.push({
                text: $(this).val(),
                displayOrder: index + 1
            });
        });
    }

    $.ajax({
        url: `/api/surveys/${surveyId}/questions`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(question),
        success: function () {
            $('#addQuestionModal').modal('hide');
            location.reload();
        },
        error: function (error) {
            alert('Error saving question: ' + error.responseText);
        }
    });
}

// Edit a question
function editQuestion(questionId) {
    const surveyId = $('#surveyId').val();

    $.ajax({
        url: `/api/surveys/${surveyId}/questions/${questionId}`,
        type: 'GET',
        success: function (question) {
            $('#editQuestionId').val(question.id);
            $('#editQuestionText').val(question.text);
            $('#editQuestionDescription').val(question.description);
            $('#editQuestionType').val(question.type);
            $('#editIsRequired').prop('checked', question.isRequired);

            // Handle options
            $('#editOptionsList').empty();
            if (question.type === 'SingleChoice' || question.type === 'MultipleChoice') {
                $('#editOptionsContainer').removeClass('d-none');

                if (question.options && question.options.length > 0) {
                    question.options.forEach(function (option) {
                        var optionHtml = `
                            <div class="input-group mb-2 option-item">
                                <input type="hidden" class="option-id" value="${option.id}">
                                <input type="text" class="form-control option-text" value="${option.text}" required>
                                <button class="btn btn-outline-danger remove-option-btn" type="button">
                                    <i class="bi bi-x"></i>
                                </button>
                            </div>
                        `;
                        $('#editOptionsList').append(optionHtml);
                    });
                }
            } else {
                $('#editOptionsContainer').addClass('d-none');
            }
        },
        error: function (error) {
            alert('Error loading question: ' + error.responseText);
        }
    });
}

// Update a question
function updateQuestion() {
    const surveyId = $('#surveyId').val();
    const questionId = $('#editQuestionId').val();

    const question = {
        id: questionId,
        text: $('#editQuestionText').val(),
        description: $('#editQuestionDescription').val(),
        isRequired: $('#editIsRequired').is(':checked'),
        options: []
    };

    const questionType = $('#editQuestionType').val();
    if (questionType === 'SingleChoice' || questionType === 'MultipleChoice') {
        $('#editOptionsList .option-item').each(function (index) {
            const optionId = $(this).find('.option-id').val();
            const optionText = $(this).find('.option-text').val();

            const option = {
                text: optionText,
                displayOrder: index + 1
            };

            if (optionId) {
                option.id = optionId;
            }

            question.options.push(option);
        });
    }

    $.ajax({
        url: `/api/surveys/${surveyId}/questions/${questionId}`,
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(question),
        success: function () {
            $('#editQuestionModal').modal('hide');
            location.reload();
        },
        error: function (error) {
            alert('Error updating question: ' + error.responseText);
        }
    });
}

// Delete a question
function deleteQuestion(questionId) {
    const surveyId = $('#surveyId').val();

    $.ajax({
        url: `/api/surveys/${surveyId}/questions/${questionId}`,
        type: 'DELETE',
        success: function () {
            location.reload();
        },
        error: function (error) {
            alert('Error deleting question: ' + error.responseText);
        }
    });
}

// Copy survey link to clipboard
function copySurveyLink() {
    const surveyId = $('#surveyId').val();
    const surveyUrl = `${window.location.origin}/Surveys/Answer/${surveyId}`;

    navigator.clipboard.writeText(surveyUrl).then(function () {
        alert('Survey link copied to clipboard!');
    }, function () {
        alert('Failed to copy survey link.');
    });
}

// Update question order
function updateQuestionOrder() {
    const surveyId = $('#surveyId').val();
    const questions = [];

    $('#questionList .list-group-item').each(function (index) {
        questions.push({
            id: $(this).data('id'),
            displayOrder: index + 1
        });
    });

    $.ajax({
        url: `/api/surveys/${surveyId}/questions/reorder`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(questions),
        error: function (error) {
            console.error('Error updating question order:', error);
        }
    });
}

// Initialize on document ready
$(document).ready(function () {
    initSurveyEditor();
});
