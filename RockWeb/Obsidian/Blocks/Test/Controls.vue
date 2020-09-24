<template>
    <RockBlock :context="context">
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-flask"></i>
                    Obsidian Control Gallery
                </h1>
            </div>
            <div class="panel-body">
                <div class="block-content">
                    <div class="row">
                        <div class="col-sm-12 col-md-6 col-lg-4">
                            <RockDefinedTypePicker label="Defined Type" v-model="definedTypeGuid" />
                            <RockDefinedValuePicker label="Defined Value" v-model="definedValueGuid" :defined-type-guid="definedTypeGuid" />
                        </div>
                    </div>
                    <hr />
                    <div class="row">
                        <div class="col-sm-12">
                            <p><strong>Defined Type Guid</strong> {{definedTypeGuid}}</p>
                            <p><strong>Defined Value Guid</strong> {{definedValueGuid}}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </RockBlock>
</template>

<script>
    export default {
        name: 'Test_Controls',
        components: {
            RockBlock: Obsidian.Controls.RockBlock,
            RockDefinedTypePicker: Obsidian.Controls.RockDefinedTypePicker,
            RockDefinedValuePicker: Obsidian.Controls.RockDefinedValuePicker
        },
        data() {
            return {
                context: Obsidian.Blocks['Test/Controls'],
                definedTypeGuid: '',
                definedValueGuid: ''
            };
        }
    };
</script>