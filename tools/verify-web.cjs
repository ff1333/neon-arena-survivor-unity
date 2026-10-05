const http = require('http');
const fs = require('fs');
const path = require('path');
const { chromium } = require('playwright');

async function main() {
  const game = process.argv[2];
  const root = path.resolve(process.argv[3]);
  const output = path.resolve(process.argv[4]);
  fs.mkdirSync(output, {recursive:true});
  const errors = [];
  const server = http.createServer((req,res) => {
    const file = path.resolve(root,'.' + decodeURIComponent(new URL(req.url,'http://localhost').pathname));
    const target = file === root ? path.join(root,'index.html') : file;
    if (!target.startsWith(root + path.sep) || !fs.existsSync(target)) {res.writeHead(404);res.end();return;}
    res.setHeader('Content-Type', ({'.js':'application/javascript','.wasm':'application/wasm','.html':'text/html','.css':'text/css','.png':'image/png'})[path.extname(target)] || 'application/octet-stream');
    fs.createReadStream(target).pipe(res);
  });
  await new Promise(resolve => server.listen(0,'127.0.0.1',resolve));
  let browser;
  try {
    browser = await chromium.launch({channel:'msedge',headless:true,args:['--enable-unsafe-swiftshader','--use-angle=swiftshader']});
    const page = await browser.newPage({viewport:{width:1280,height:800}});
    page.on('pageerror',e => errors.push(e.message));
    page.on('console',m => {if (/Exception|Shader error|Aborted/i.test(m.text())) errors.push(m.text());});
    await page.goto(`http://127.0.0.1:${server.address().port}/`);
    await page.waitForFunction(() => {
      const bar=document.querySelector('#unity-loading-bar');
      return bar && bar.style.display==='none';
    },null,{timeout:90000});
    await page.waitForTimeout(2000);
    const canvas=page.locator('canvas');
    const box=await canvas.boundingBox();
    const click=async(x,y) => {await page.mouse.click(box.x+box.width*x,box.y+box.height*y);await page.waitForTimeout(600);};
    await page.screenshot({path:path.join(output,game+'-web-title.png')});
    await click(game==='fogbound'?.48:.22,game==='fogbound'?.855:.785);
    await page.screenshot({path:path.join(output,game+'-web-settings-zh.png')});
    await click(.75,.265);
    await click(.60,.52);
    await click(.80,.395);
    await page.screenshot({path:path.join(output,game+'-web-settings-en-muted.png')});
    await click(.24,.90);
    await click(game==='fogbound'?.48:.22,game==='fogbound'?.855:.785);
    await page.screenshot({path:path.join(output,game+'-web-settings-reopened.png')});
    await click(.53,.265);
    await click(.80,.395);
    await click(.24,.90);
    if(game==='fogbound') {
      await click(.20,.745);
      await click(.25,.34);
      await page.screenshot({path:path.join(output,game+'-web-guide.png')});
      await click(.15,.89);
      await click(.20,.635);
      await click(.15,.385);
      await page.screenshot({path:path.join(output,game+'-web-loadout.png')});
      await click(.36,.55);
    } else {
      await click(.22,.65);
      await page.screenshot({path:path.join(output,game+'-web-loadout.png')});
      await click(.26,.52);
    }
    await page.keyboard.down('w');await page.waitForTimeout(900);await page.keyboard.up('w');
    await page.screenshot({path:path.join(output,game+'-web-gameplay.png')});
    if(game==='fogbound') {
      await page.mouse.down();await page.waitForTimeout(250);await page.mouse.up();
      await page.keyboard.press('r');
      await page.waitForTimeout(500);
      await page.screenshot({path:path.join(output,game+'-web-reloading.png')});
      await page.waitForTimeout(950);
      await page.screenshot({path:path.join(output,game+'-web-ready.png')});
    }
    if(game==='fogbound') await page.keyboard.press('Escape');
    else await click(.973,.04);
    await page.waitForTimeout(500);
    await page.screenshot({path:path.join(output,game+'-web-pause.png')});
    fs.writeFileSync(path.join(output,game+'-web-result.json'),JSON.stringify({game,errors,canvas:box,screenshots:fs.readdirSync(output).filter(name=>name.endsWith('.png')),note:'Automated Edge interaction; screenshots require visual review. Not an Android device test or subjective audio audition.'},null,2));
    if(errors.length) throw new Error(errors.join('\n'));
    console.log(game+': browser loaded and interaction sequence completed');
  } finally {
    if(browser) await browser.close();
    await new Promise(resolve => server.close(resolve));
  }
}
main().catch(error=>{console.error(error);process.exitCode=1;});
