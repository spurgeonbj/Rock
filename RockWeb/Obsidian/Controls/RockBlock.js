Obsidian.Controls.RockBlock = {
    props: {
        context: {
            type: Object,
            required: true
        }
    },
    provide: function () {
        return this.context;
    },
    template:
`<div class="obsidian-block">
    <slot />
</div>`
};
