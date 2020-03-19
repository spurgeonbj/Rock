<template>
    <div class="panel panel-block obsidian-block">
        <div class="panel-heading">
            <h1 class="panel-title">
                <i class="fa fa-flask"></i>
                Obsidian Control Test
            </h1>
        </div>
        <div class="panel-body">
            <div class="block-content">
                <rock-definedtypepicker label="Defined Type" v-model="definedTypeGuid" />
                <rock-definedvaluepicker label="Defined Value" v-model="definedValueGuid" :defined-type-guid="definedTypeGuid" />
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'Test_Controls',
        data() {
            return {
                definedTypeGuid: '',
                definedValueGuid: ''
            };
        }
    };
</script>