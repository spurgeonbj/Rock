Vue.component('rock-definedtypepicker', {
    props: {
        value: {
            type: String,
            required: true
        },
        label: {
            type: String,
            required: true
        },
        required: {
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            uniqueId: `rock-definedtypepicker-${Obsidian.Util.getGuid()}`,
            internalValue: this.value,
            definedTypes: [],
            isLoading: false
        };
    },
    methods: {
        onChange: function () {
            this.$emit('input', this.internalValue);
            this.$emit('change', this.internalValue);
        }
    },
    watch: {
        value: function () {
            this.internalValue = this.value;
        }
    },
    created: async function () {
        this.isLoading = true;
        const result = await this.$http.get(
            '/api/DefinedTypes?$filter=IsActive eq true&$select=Guid,Name');

        if (result && Array.isArray(result.data)) {
            this.definedTypes = result.data;
        }

        this.isLoading = false;
    },
    template:
`<div class="form-group defined-type-picker" :class="{required: required}">
    <label class="control-label" :for="uniqueId">{{label}}</label>
    <div class="control-wrapper">
        <select :id="uniqueId" class="form-control" v-model="internalValue" @change="onChange" :disabled="isLoading">
            <option value=""></option>
            <option v-for="dt in definedTypes" :key="dt.Guid" :value="dt.Guid">{{dt.Name}}</option>
        </select>
    </div>
</div>`
});
