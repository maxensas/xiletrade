'use strict';

// Firefox : browser.* (promises), Chrome : chrome.* (promises in Manifest V3)
const api = globalThis.browser ?? globalThis.chrome;
const t = (key, ...subs) => api.i18n.getMessage(key, subs) || key;

// sites of the session cookie : same list as the trade gateways of Xiletrade
const REALMS = [
  { id: 'pc', site: 'pathofexile.com', url: 'https://www.pathofexile.com', origin: 'https://*.pathofexile.com/*' },
  { id: 'kakao', site: 'poe.kakaogames.com', url: 'https://poe.kakaogames.com', origin: 'https://poe.kakaogames.com/*' },
  { id: 'tw', site: 'pathofexile.tw', url: 'https://pathofexile.tw', origin: 'https://pathofexile.tw/*' },
  { id: 'tencent', site: 'poe.game.qq.com', url: 'https://poe.game.qq.com', origin: 'https://poe.game.qq.com/*' }
];
const SESSION_ID = /^[0-9a-f]{32}$/i;

const list = document.getElementById('realms');
const allow = document.getElementById('allow');
const status = document.getElementById('status');

function setStatus(text, kind = '') {
  status.textContent = text;
  status.className = kind;
}

function translate() {
  for (const el of document.querySelectorAll('[data-i18n]')) {
    el.textContent = t(el.dataset.i18n);
  }
  document.documentElement.lang = api.i18n.getUILanguage();
}

async function getSession(realm) {
  // the value is read on demand and never stored by the extension
  const cookie = await api.cookies.get({ url: realm.url, name: 'POESESSID' });
  return cookie && SESSION_ID.test(cookie.value) ? cookie.value : null;
}

async function send(realm) {
  const id = await getSession(realm);
  if (!id) {
    setStatus(t('notFound'), 'error');
    return;
  }
  // handled by the protocol handler of Xiletrade : the running instance receives it through its local pipe
  const url = `xiletrade://session?id=${id}&realm=${realm.id}`;
  try {
    // an external protocol does not navigate the tab : the browser asks to open Xiletrade
    const [tab] = await api.tabs.query({ active: true, currentWindow: true });
    await api.tabs.update(tab.id, { url });
  } catch {
    await api.tabs.create({ url, active: false });
  }
  setStatus(t('sent'), 'ok');
}

function row(realm, id) {
  const li = document.createElement('li');
  const info = document.createElement('div');
  const site = document.createElement('span');
  site.className = 'site';
  site.textContent = realm.site;
  const state = document.createElement('span');
  state.className = id ? 'state ok' : 'state';
  info.append(site, state);

  if (id) {
    state.textContent = t('found');
    const button = document.createElement('button');
    button.textContent = t('send');
    button.addEventListener('click', () => send(realm));
    li.append(info, button);
  } else {
    state.textContent = t('notFound') + ' ';
    const link = document.createElement('a');
    link.href = realm.url;
    link.target = '_blank';
    link.textContent = t('login');
    state.append(link);
    li.append(info);
  }
  return li;
}

async function render() {
  const origins = REALMS.map(r => r.origin);
  const granted = await api.permissions.contains({ origins });
  allow.hidden = granted;
  list.replaceChildren();
  if (!granted) {
    setStatus(t('needAccess'));
    return;
  }
  setStatus('');
  const sessions = await Promise.all(REALMS.map(getSession));
  REALMS.forEach((realm, i) => list.append(row(realm, sessions[i])));
}

// Firefox can install the host permissions as optional : asked on a click
allow.addEventListener('click', async () => {
  const origins = REALMS.map(r => r.origin);
  if (await api.permissions.request({ origins })) {
    await render();
  }
});

translate();
render().catch(e => setStatus(String(e), 'error'));
