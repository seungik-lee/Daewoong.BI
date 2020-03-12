/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
	
	config.language = 'ko';
	config.height = 300;
	config.enterMode = CKEDITOR.ENTER_BR;	// 줄바꿈 모드를 BR 태그로 사용.
	//config.fillEmptyBlocks = false;	// 줄바꿈을 P태그로 사용시 자동으로 공백문자 추가하는 기능 방지.
	config.allowedContent = true;	// true: 소스 편집 모드에서 작성한 html 을 CKEditor 가 자동으로 변경하지 않게 함.
	
	//config.font_names = '굴림; 돋움; 궁서; HY견고딕; HY견명조; 휴먼둥근헤드라인;' 
    //    + '휴먼매직체; 휴먼모음T; 휴먼아미체; 휴먼엑스포; 휴먼옛체; 휴먼편지체;' 
    //    +  CKEDITOR.config.font_names;
	
	//config.font_names = '돋움; 굴림; 고딕; 바른고딕;';
	config.font_names = '돋움; 굴림; 명조; 고딕; 궁서; 맑은고딕; 나눔고딕; 나눔바른고딕;' + CKEDITOR.config.font_names;
		
	//config.font_names = 'Malgun Gothic';
	
	config.toolbarGroups = [
		{ name: 'document', groups: [ 'mode', 'document', 'doctools' ] },
		{ name: 'clipboard', groups: [ 'clipboard', 'undo' ] },
		{ name: 'editing', groups: [ 'find', 'selection', 'spellchecker', 'editing' ] },
		{ name: 'forms', groups: [ 'forms' ] },
		'/',
		{ name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },
		{ name: 'paragraph', groups: [ 'list', 'indent', 'align', 'blocks', 'bidi', 'paragraph' ] },
		{ name: 'links', groups: [ 'links' ] },
		{ name: 'insert', groups: [ 'insert' ] },
		'/',
		{ name: 'styles', groups: [ 'styles' ] },
		{ name: 'colors', groups: [ 'colors' ] },
		{ name: 'tools', groups: [ 'tools' ] },
		{ name: 'others', groups: [ 'others' ] },
		{ name: 'about', groups: [ 'about' ] }
	];

	//config.removeButtons = 'Source,Save,Templates,NewPage,Preview,Print,Cut,Undo,Redo,Copy,Paste,PasteText,PasteFromWord,Replace,Find,SelectAll,Scayt,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Strike,Subscript,Superscript,RemoveFormat,NumberedList,BulletedList,Indent,Outdent,CreateDiv,JustifyBlock,Language,BidiRtl,BidiLtr,Anchor,Flash,Table,HorizontalRule,Smiley,PageBreak,Iframe,Styles,Format,Font,FontSize,BGColor,TextColor,ShowBlocks,Maximize,About';
	//config.removeButtons = 'Save,Templates,NewPage,Preview,Print,Cut,Undo,Redo,Copy,Paste,PasteText,PasteFromWord,Replace,Find,SelectAll,Scayt,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Strike,Subscript,Superscript,RemoveFormat,NumberedList,BulletedList,Indent,Outdent,CreateDiv,JustifyBlock,Language,BidiRtl,BidiLtr,Anchor,Flash,Table,HorizontalRule,Smiley,PageBreak,Iframe,Styles,Format,Font,FontSize,BGColor,TextColor,ShowBlocks,Maximize,About';
	//config.removeButtons = 'Save,Templates,NewPage,Preview,Print,Cut,Undo,Redo,Copy,Paste,PasteText,PasteFromWord,Replace,Find,SelectAll,Scayt,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,RemoveFormat,CreateDiv,JustifyBlock,Language,BidiRtl,BidiLtr,Anchor,Flash,Smiley,PageBreak,Iframe,ShowBlocks,Maximize,About';
	config.removeButtons = 'Save,Form,Checkbox,Radio,TextField,Textarea,Select,Button,ImageButton,HiddenField,Flash,PageBreak,Iframe,Maximize,Print,Preview,About,Scayt,ShowBlocks,Anchor,Image';
};

