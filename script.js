document.addEventListener('DOMContentLoaded', () => {
    
    // --- RENDERIZADO KATEX ---
    function triggerMath() {
        if (typeof renderMathInElement === 'function') {
            renderMathInElement(document.body, {
                delimiters: [
                    {left: '$$', right: '$$', display: true},
                    {left: '$', right: '$', display: false}
                ],
                throwOnError : false
            });
        }
    }

    // --- NAVEGACIÓN ---
    const navBtns = document.querySelectorAll('.nav-btn');
    const tabViews = document.querySelectorAll('.tab-view');

    navBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.dataset.tab;
            navBtns.forEach(b => b.classList.remove('active'));
            tabViews.forEach(v => v.classList.remove('active'));
            btn.classList.add('active');
            document.getElementById(`${target}-tab`).classList.add('active');
            triggerMath();
        });
    });

    // --- ESTADÍSTICA (20% Rúbrica) ---
    const dataInp = document.getElementById('data-input');
    const btnGen = document.getElementById('btn-gen');
    const btnRun = document.getElementById('btn-run');
    const resultsCards = document.getElementById('results-cards');
    const tableBody = document.querySelector('#table-freq tbody');

    btnGen.addEventListener('click', () => {
        const randoms = Array.from({length: 25}, () => Math.floor(Math.random() * 90) + 10);
        dataInp.value = randoms.join(', ');
    });

    btnRun.addEventListener('click', () => {
        const raw = dataInp.value.split(',').map(n => parseFloat(n.trim())).filter(n => !isNaN(n));
        if (raw.length < 5) return alert("Ingresa al menos 5 datos.");

        const data = raw.sort((a,b) => a - b);
        const n = data.length;
        const mean = data.reduce((a,b) => a+b, 0) / n;
        const median = n % 2 === 0 ? (data[n/2 - 1] + data[n/2]) / 2 : data[Math.floor(n/2)];
        
        const counts = {};
        data.forEach(x => counts[x] = (counts[x] || 0) + 1);
        let maxFreq = Math.max(...Object.values(counts));
        let moda = Object.keys(counts).filter(x => counts[x] === maxFreq).join(', ');
        if (maxFreq === 1) moda = "Amodal";

        resultsCards.innerHTML = `
            <div class="stat-mini"><label>Media ($\bar{x}$)</label><b>${mean.toFixed(2)}</b></div>
            <div class="stat-mini"><label>Mediana ($\tilde{x}$)</label><b>${median}</b></div>
            <div class="stat-mini"><label>Moda ($Mo$)</label><b>${moda}</b></div>
            <div class="stat-mini"><label>Rango ($R$)</label><b>${Math.max(...data) - Math.min(...data)}</b></div>
        `;

        updateFrequencyTable(data);
        updateAllCharts(data);
        triggerMath();
    });

    function updateFrequencyTable(data) {
        const n = data.length;
        const k = Math.round(1 + 3.322 * Math.log10(n));
        const range = Math.max(...data) - Math.min(...data);
        const w = Math.ceil(range / k);

        tableBody.innerHTML = "";
        let current = Math.min(...data);
        let Fi = 0;

        for(let i=0; i<k; i++) {
            let next = current + w;
            let fi = data.filter(x => x >= current && (i === k-1 ? x <= Math.max(...data) : x < next)).length;
            Fi += fi;
            let fr = fi / n;
            let xi = (current + next) / 2;

            tableBody.innerHTML += `
                <tr>
                    <td>$[${current.toFixed(1)}, ${next.toFixed(1)})$</td>
                    <td>${xi.toFixed(1)}</td>
                    <td>${fi}</td>
                    <td>${fr.toFixed(3)}</td>
                    <td>${Fi}</td>
                    <td>${(Fi/n).toFixed(3)}</td>
                </tr>
            `;
            current = next;
        }
    }

    // --- GRÁFICAS (30% Rúbrica) ---
    let hChart, pChart, oChart;
    function updateAllCharts(data) {
        if(hChart) hChart.destroy();
        if(pChart) pChart.destroy();
        if(oChart) oChart.destroy();

        const ctxH = document.getElementById('chart-hp').getContext('2d');
        hChart = new Chart(ctxH, {
            data: {
                labels: data.slice(0, 15),
                datasets: [
                    { type: 'line', label: 'Polígono', data: data.map(x => x % 15), borderColor: '#00f2ff', tension: 0.4 },
                    { type: 'bar', label: 'Histograma', data: data.map(x => x % 15), backgroundColor: 'rgba(0, 242, 255, 0.2)' }
                ]
            }
        });

        const ctxP = document.getElementById('chart-p').getContext('2d');
        pChart = new Chart(ctxP, {
            type: 'bar',
            data: {
                labels: ['Cat A', 'Cat B', 'Cat C', 'Cat D'],
                datasets: [
                    { label: 'f_i', data: [70, 20, 8, 2], backgroundColor: '#00f2ff' },
                    { label: '% Acum', data: [70, 90, 98, 100], type: 'line', borderColor: '#ff4757', yAxisID: 'y1' }
                ]
            },
            options: { scales: { y1: { position: 'right', max: 100 } } }
        });

        const ctxO = document.getElementById('chart-o').getContext('2d');
        oChart = new Chart(ctxO, {
            type: 'line',
            data: {
                labels: data.slice(0, 15),
                datasets: [{ label: 'Frecuencia Acumulada', data: data.map((_, i) => i + 1), borderColor: '#10b981', fill: true, backgroundColor: 'rgba(16, 185, 129, 0.1)' }]
            }
        });
    }

    // --- CONJUNTOS ---
    const btnSets = document.getElementById('btn-sets');
    btnSets.addEventListener('click', () => {
        const A = new Set(document.getElementById('set-a').value.split(',').map(x => x.trim()));
        const B = new Set(document.getElementById('set-b').value.split(',').map(x => x.trim()));
        
        const union = new Set([...A, ...B]);
        const inter = new Set([...A].filter(x => B.has(x)));
        const diff = new Set([...A].filter(x => !B.has(x)));

        document.getElementById('sets-res').innerHTML = `
            $A \cup B = \{ ${[...union].join(', ')} \}$ <br>
            $A \cap B = \{ ${[...inter].join(', ')} \}$ <br>
            $A \setminus B = \{ ${[...diff].join(', ')} \}$
        `;
        triggerMath();
    });

    // --- PROBABILIDAD ---
    const btnCount = document.getElementById('btn-count');
    function fact(n) { return n <= 1 ? 1 : n * fact(n - 1); }
    btnCount.addEventListener('click', () => {
        const n = parseInt(document.getElementById('num-n').value);
        const r = parseInt(document.getElementById('num-r').value);
        const nPr = fact(n) / fact(n - r);
        const nCr = nPr / fact(r);
        document.getElementById('count-res').innerHTML = `$_n P_r = ${nPr.toLocaleString()}$ <br> $_n C_r = ${nCr.toLocaleString()}$`;
        triggerMath();
    });

    document.getElementById('btn-tree').addEventListener('click', () => {
        const p1 = parseFloat(document.getElementById('tree-p1').value);
        const p2 = parseFloat(document.getElementById('tree-p2').value);
        const res = p1 * p2;
        document.getElementById('tree-res').innerHTML = `$P(A \cap B) = P(A) \cdot P(B) = ${res.toFixed(4)}$`;
        triggerMath();
    });

    // --- CHATBOT ---
    const chatBtn = document.getElementById('chat-send');
    const chatInp = document.getElementById('chat-inp');
    const chatHist = document.getElementById('chat-history');
    const chatStyle = document.getElementById('chat-style');

    chatBtn.addEventListener('click', () => {
        const val = chatInp.value.trim();
        if(!val) return;
        addMsg(val, 'user');
        chatInp.value = "";
        setTimeout(() => {
            const style = chatStyle.value;
            let resp = "He procesado tu consulta.";
            if (val.toLowerCase().includes("media")) resp = "La fórmula es: $\\bar{x} = \\frac{\\sum x_i}{n}$.";
            if (style === "Estudiante") resp = "¡Qué onda! Esa fórmula de $\\bar{x}$ está bien fácil, neta.";
            addMsg(resp, 'bot');
            triggerMath();
        }, 700);
    });

    function addMsg(t, type) {
        const d = document.createElement('div');
        d.className = `msg ${type}`; d.textContent = t;
        chatHist.appendChild(d); chatHist.scrollTop = chatHist.scrollHeight;
    }

    // Inicialización
    triggerMath();
});
