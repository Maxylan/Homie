// (c) 2024 @Maxylan
document.addEventListener(
    'DOMContentLoaded', 
    async () => (b = document
        .querySelector('body'))
        .addEventListener('load', 
            configure_ui(b/*...*/)
            .catch(errors)
            .finally(say_goodbye)
        )
);

const configure_ui = async (b/*: HTMLBodyElement*/) => {

    let index = 0,
        timeout = 10;
    var models; // Brute-forcing my way into the UI!

    // Await Swagger UI (react) to render.
    while (++index < 499 && !(models = b.querySelector('section.models'))) {
        timeout = 25 + (timeout * index) / 4;
        console.debug('#'+index, 'timeout: ' + timeout + 'ms -', models);
        await new Promise(r => setTimeout(r, timeout));
    }
    
    models.remove();

    // Append another to the description, leading to this project's repository.
    b.querySelector('div.info__contact div').innerHTML += ` - <a href="https://github.com/Maxylan/Homie" target="_blank" rel="noopener noreferrer" class="link">Repository</a>`;

    // Append another to the description, leading to this project's repository. // `'span#homie-version'`
    const version = b.querySelector('pre.version')?.textContent?.trim() ?? '? (Unknown)';
    console.log(('%c Homie v'+version), "font-weight:bold;color:grey;font-size: 14px;");

    // Highlight `development` in paths as red text.
    b.querySelectorAll('.opblock-summary-path')
        .forEach(summary => summary.querySelectorAll('span')
            .forEach(
                e => {
                    e.innerHTML = e.innerHTML.replace('development', '<strong style="color:red;">development</strong>');
                    e.classList.add('development');
                }
            )
        );

    // Add a button that allows you to toggle the visiblity of the `development` paths.
    b.querySelector('.information-container div.info').innerHTML += '<br/>' + (
        `<label for="development-visibility"><input type="checkbox" name="development-visibility" id="development-visibility"/> - Hide "Development" actions/endpoints.</label>`
    ).trim();

    // Add a listener to the checkbox.
    b.querySelector('#development-visibility').addEventListener('change', e => {
        b.querySelectorAll('div.opblock-tag-section')
            .forEach(tagSection => {
                const tag = tagSection.querySelector('.opblock-tag');
                if (tag.dataset.tag.match(/[Dd]evelopment/)) {
                    tagSection.style.display = e.target.checked ? 'none' : 'inline';
                }
            });
    });

    // Reset the checkbox when the "filter" input-field changes.
    /* b.querySelector('.operation-filter-input').addEventListener(
        'change', e => b.querySelector('#development-visibility').checked = false
    ); */
};

const errors = (err) => { 
    let consoleStyle = "font-weight: bold;";
    console.error('%c Backoffice.js encountered error.', consoleStyle, err);
    console.error(err);
};

const say_goodbye = () => { 
    let consoleStyle = "font-size: 14px;";
    console.log('%c Ⓒ 2024 @Maxylan', consoleStyle + "font-weight:bold;color:grey;");
    console.log('%c Backoffice.js loaded.', consoleStyle);
};
