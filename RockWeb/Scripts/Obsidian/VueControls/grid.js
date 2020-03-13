Vue.component('rock-grid', {
    template:
`<div class="grid grid-panel">
    <slot name="filter" />
    <div class="table-responsive">
        <table class="grid-table table table-bordered table-striped table-hover">
            <thead>
                <slot name="thead" />
            </thead>
            <tbody>
                <slot />
            </tbody>
            <tfoot>
                <tr>
                    <td class="grid-paging" colspan="6">
                        <ul class="grid-pagesize pagination pagination-sm">
                            <li class="active">50</li>
                            <li class="active">500</li>
                            <li class="active">5,000</li>
                        </ul>
                        <div class="grid-itemcount">5 Groups</div>
                    </td>
                </tr>
                <tr>
                    <td class="grid-actions">
                    </td>
                </tr>
            </tfoot>
        </table>
    </div>
</div>`
});
