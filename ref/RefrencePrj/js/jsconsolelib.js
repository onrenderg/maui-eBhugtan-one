// JS Console Library - Import in any HTML page
(function () {
    // Inject CSS
    const style = document.createElement('style');
    style.textContent = `
        #jsConsole {
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            height: 40vh;
            background: #1e1e1e;
            border-top: 2px solid #007acc;
            font-family: 'Consolas', monospace;
            display: flex;
            flex-direction: column;
            z-index: 9999;
        }
        #consoleOutput {
            flex: 1;
            overflow-y: auto;
            padding: 10px;
            color: #d4d4d4;
        }
        #consoleOutput .log { color: #d4d4d4; }
        #consoleOutput .error { color: #f48771; }
        #consoleOutput .result { color: #4ec9b0; }
        #consoleOutput .input-echo { color: #9cdcfe; }
        #consoleInput {
            display: flex;
            background: #252526;
            padding: 8px;
            align-items: flex-start;
        }
        #consoleInput span {
            color: #007acc;
            margin-right: 8px;
            margin-top: 4px;
        }
        #consoleInput textarea {
            flex: 1;
            background: transparent;
            border: none;
            color: #d4d4d4;
            font-family: inherit;
            font-size: 14px;
            outline: none;
            resize: none;
            overflow: hidden;
            min-height: 20px;
            max-height: 120px;
            line-height: 1.4;
        }
        body { margin-bottom: 42vh; }
    `;
    document.head.appendChild(style);

    // Inject HTML
    const consoleDiv = document.createElement('div');
    consoleDiv.id = 'jsConsole';
    consoleDiv.innerHTML = `
        <div id="consoleOutput"></div>
        <div id="consoleInput">
            <span>❯</span>
            <textarea id="cmdInput" rows="1" placeholder="Type JS here... (Shift+Enter for new line)" autofocus></textarea>
        </div>
    `;
    document.body.appendChild(consoleDiv);

    // Console logic
    const output = document.getElementById('consoleOutput');
    const input = document.getElementById('cmdInput');
    const history = [];
    let historyIndex = -1;

    function log(text, type = 'log') {
        const div = document.createElement('div');
        div.className = type;
        div.textContent = text;
        output.appendChild(div);
        output.scrollTop = output.scrollHeight;
    }

    // Override console.log
    const originalLog = console.log;
    console.log = function (...args) {
        originalLog.apply(console, args);
        log(args.map(a => typeof a === 'object' ? JSON.stringify(a) : a).join(' '), 'log');
    };

    // Auto-resize textarea
    function autoResize() {
        input.style.height = 'auto';
        input.style.height = Math.min(input.scrollHeight, 120) + 'px';
    }
    input.addEventListener('input', autoResize);

    input.addEventListener('keydown', function (e) {
        // Shift+Enter = new line (default behavior, don't intercept)
        // Enter alone = execute
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            const cmd = input.value.trim();
            if (!cmd) return;

            history.push(cmd);
            historyIndex = history.length;
            log('❯ ' + cmd, 'input-echo');

            try {
                // Convert let/const to var so variables persist between executions
                // (0, eval) = indirect eval = runs in GLOBAL scope (not inside this IIFE)
                const execCmd = cmd.replace(/^\s*(let|const)\s+/gm, 'var ');
                const result = (0, eval)(execCmd);
                if (result !== undefined) {
                    log('← ' + (typeof result === 'object' ? JSON.stringify(result, null, 2) : result), 'result');
                }
            } catch (err) {
                log('✖ ' + err.message, 'error');
            }

            input.value = '';
            autoResize();
        }
        if (e.key === 'ArrowUp' && historyIndex > 0 && !input.value.includes('\n')) {
            historyIndex--;
            input.value = history[historyIndex];
            autoResize();
        }
        if (e.key === 'ArrowDown' && historyIndex < history.length - 1 && !input.value.includes('\n')) {
            historyIndex++;
            input.value = history[historyIndex];
            autoResize();
        }
    });

    log('🚀 JS Console Ready!', 'result');
})();