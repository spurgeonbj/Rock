Obsidian.Blocks['Test/Controls'] = ({ rootElement }) => {
    new Vue({
        el: rootElement,
        data() {
            return {
                definedTypeGuid: '',
                definedValueGuid: ''
            };
        },
        template:
`<div class="panel panel-block obsidian-block">
    <div class="panel-heading">
        <h1 class="panel-title">
            <i class="fa fa-flask"></i>
            Obsidian Control Test
        </h1>
    </div>
    <div class="panel-body">
	    <div class="block-content">
            <rock-definedtypepicker label="Defined Type" v-model="definedTypeGuid"></rock-definedtypepicker>
            <rock-definedvaluepicker label="Defined Value" v-model="definedValueGuid" :defined-type-guid="definedTypeGuid"></rock-definedvaluepicker>
        </div>
    </div>
</div>`
    });
};
