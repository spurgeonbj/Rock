Vue.component('rock-definedvaluepicker', {
    props: {
        value: {
            type: String,
            required: true
        },
        label: {
            type: String,
            required: true
        },
        definedTypeGuid: {
            type: String,
            default: ''
        },
        required: {
            type: Boolean,
            default: false
        }
    },
    data: function () {
        return {
            uniqueId: `rock-definedvaluepicker-${Obsidian.Util.getGuid()}`,
            internalValue: this.value,
            definedValues: [],
            isLoading: false
        };
    },
    computed: {
        isEnabled: function () {
            return !!this.definedTypeGuid && !this.isLoading;
        }
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
        },
        definedTypeGuid: {
            immediate: true,
            handler: async function () {
                if (!this.definedTypeGuid) {
                    this.definedTypes = [];
                    return;
                }

                this.isLoading = true;
                const result = await this.$http.get(
                    `/api/DefinedValues?$filter=IsActive eq true and DefinedType/Guid eq guid'${this.definedTypeGuid}'&$select=Value,Guid`);

                if (result && Array.isArray(result.data)) {
                    this.definedValues = result.data;
                }

                this.isLoading = false;
            }
        }
    },
    template:
`<div class="form-group defined-value-picker" :class="{required: required}">
    <label class="control-label" :for="uniqueId">{{label}}</label>
    <div class="control-wrapper">
        <select :id="uniqueId" class="form-control" v-model="internalValue" @change="onChange" :disabled="!isEnabled">
            <option value=""></option>
            <option v-for="dv in definedValues" :key="dv.Guid" :value="dv.Guid">{{dv.Value}}</option>
        </select>
    </div>
</div>`
});
