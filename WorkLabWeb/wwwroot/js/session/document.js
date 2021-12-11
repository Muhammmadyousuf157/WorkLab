DecoupledEditor
	.create(document.querySelector('#editor'), { removePlugins: ['MediaEmbed'] })
	.then(editor => {
		const toolbarContainer = document.querySelector('main .toolbar-container');

		toolbarContainer.prepend(editor.ui.view.toolbar.element);

		window.editor = editor;

		bindEditorContentChangeEvent();

		editor.focus();

		const fileUploader = `
			<button class="ck ck-button ck-off" type="button" tabindex="-1">
				<svg class="ck ck-icon ck-button__icon" viewBox="0 0 20 20">
					<path d="M6.91 10.54c.26-.23.64-.21.88.03l3.36 3.14 2.23-2.06a.64.64 0 0 1 .87 0l2.52 2.97V4.5H3.2v10.12l3.71-4.08zm10.27-7.51c.6 0 1.09.47 1.09 1.05v11.84c0 .59-.49 1.06-1.09 1.06H2.79c-.6 0-1.09-.47-1.09-1.06V4.08c0-.58.49-1.05 1.1-1.05h14.38zm-5.22 5.56a1.96 1.96 0 1 1 3.4-1.96 1.96 1.96 0 0 1-3.4 1.96z"></path>
				</svg>
				<span class="ck ck-tooltip ck-tooltip_s">
					<span class="ck ck-tooltip__text">Insert image</span>
				</span>
			</button>
			<input type="file" hidden />
		`;

		$('.ck-toolbar[aria-label="Bulleted list styles toolbar"]').parent().parent().remove();



		const $fileDialog = $('.ck-toolbar__items .ck-file-dialog-button');

		$fileDialog.empty();
		$fileDialog.html(fileUploader);

		$fileDialog.delegate('button', 'click', function () {
			$(this).next().trigger('click');
		});

		$fileDialog.delegate('input[type="file"]', 'change', function () {
			if (!this.files)
				return;

			const reader = new FileReader();

			reader.onload = e => {
				const imageMarkup = `
					<figure class="image ck-widget ck-widget_selected" contenteditable="false">
						<img src="${e.target.result}" />
						<figcaption class="ck-editor__editable ck-editor__nested-editable" data-placeholder="Enter image caption" contenteditable="true"></figcaption>
						<div class="ck ck-reset_all ck-widget__type-around">
							<div class="ck ck-widget__type-around__button ck-widget__type-around__button_before" title="Insert paragraph before block">
								<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 8"><path d="M9.055.263v3.972h-6.77M1 4.216l2-2.038m-2 2 2 2.038"></path></svg>
							</div>
							<div class="ck ck-widget__type-around__button ck-widget__type-around__button_after" title="Insert paragraph after block">
								<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 8"><path d="M9.055.263v3.972h-6.77M1 4.216l2-2.038m-2 2 2 2.038"></path></svg>
							</div>
							<div class="ck ck-widget__type-around__fake-caret"></div>
						</div>
					</figure>
				`;

				editor.setData(editor.getData() + imageMarkup);

				console.log(e.target.result);
			};

			reader.readAsDataURL(this.files[0]);
		});
	})
	.catch(err => {
		console.error(err.stack);
	});

async function broadcastEditorContent(event, data) {
	const editorContent = editor.getData();
	await hubConnection.invoke('SendEditorContent', editorContent, sessionKey);
}

function bindEditorContentChangeEvent() {
	editor.model.document.on('change:data', broadcastEditorContent);

	hubConnection.on('ReceiveEditorContent', editorContent => {
		editor.model.document.off('change:data', broadcastEditorContent);
		editor.setData(editorContent);
		editor.model.document.on('change:data', broadcastEditorContent);
	});
}