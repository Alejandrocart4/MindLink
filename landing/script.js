const form = document.querySelector('#waitlist-form');
const nameInput = document.querySelector('#name');
const emailInput = document.querySelector('#email');
const purposeInput = document.querySelector('#purpose');
const otherPurposeField = document.querySelector('#other-purpose-field');
const otherPurposeInput = document.querySelector('#other-purpose');
const emailError = document.querySelector('#email-error');
const purposeError = document.querySelector('#purpose-error');
const otherPurposeError = document.querySelector('#other-purpose-error');
const submissionError = document.querySelector('#submission-error');
const success = document.querySelector('#success-message');
const submitButton = form.querySelector('button[type="submit"]');
const submitButtonContent = submitButton.innerHTML;
const mindlinkVideo = document.querySelector('#mindlink-video');
const videoShowcase = document.querySelector('#video-showcase');
const videoExpandButton = document.querySelector('#video-expand-button');
const videoCloseButton = document.querySelector('#video-close-button');
const videoPlayToggle = document.querySelector('#video-play-toggle');
const videoMuteButton = document.querySelector('#video-mute-button');
const videoMuteLabel = videoMuteButton.querySelector('.video-mute-label');
const videoComment = document.querySelector('#video-comment');
const videoAnnotations = [
  { start: 0, text: 'Todo tu proceso de investigación, en un solo espacio.' },
  { start: 4, text: 'Aquí podrás revisar avances, pendientes y actividad reciente.' },
  { start: 8, text: 'Organiza y retoma rápidamente cada proyecto.' },
  { start: 12, text: 'Redacta por secciones con tus fuentes siempre a mano.' },
  { start: 16, text: 'Captura y clasifica notas sin perder su contexto.' },
  { start: 20, text: 'Visualiza conexiones entre ideas, fuentes y argumentos.' },
  { start: 24, text: 'Administra tus referencias y encuentra cada fuente.' },
  { start: 28, text: 'Vincula citas y evidencia directamente con tu investigación.' },
  { start: 32, text: 'Compara versiones y recupera cambios con confianza.' },
  { start: 36, text: 'Exporta tu documento en el formato que necesites.' },
  { start: 40, text: 'Personaliza la apariencia para trabajar cómodamente.' }
];
let activeAnnotation = -1;
let annotationAnimationFrame;
let lastVideoTrigger;

document.querySelector('#year').textContent = new Date().getFullYear();

function updateMuteControl() {
  const isMuted = mindlinkVideo.muted || mindlinkVideo.volume === 0;
  videoMuteButton.setAttribute('aria-pressed', String(isMuted));
  videoMuteLabel.textContent = isMuted ? 'Activar sonido' : 'Silenciar video';
}

function updatePlayControl() {
  const isPaused = mindlinkVideo.paused;
  videoPlayToggle.dataset.paused = String(isPaused);
  videoPlayToggle.setAttribute('aria-label', isPaused ? 'Reproducir video' : 'Pausar video');
}

function updateVideoComment() {
  const currentAnnotation = videoAnnotations.reduce((selected, annotation, index) => (
    mindlinkVideo.currentTime >= annotation.start ? index : selected
  ), 0);

  if (currentAnnotation === activeAnnotation) {
    return;
  }

  activeAnnotation = currentAnnotation;
  videoComment.textContent = videoAnnotations[currentAnnotation].text;
}

function startAnnotationSync() {
  window.cancelAnimationFrame(annotationAnimationFrame);

  const syncAnnotation = () => {
    updateVideoComment();

    if (!mindlinkVideo.paused) {
      annotationAnimationFrame = window.requestAnimationFrame(syncAnnotation);
    }
  };

  syncAnnotation();
}

async function ensureVideoIsPlaying() {
  if (!mindlinkVideo.paused) {
    return;
  }

  try {
    await mindlinkVideo.play();
  } catch (error) {
    console.info('El navegador espera una interacción para reproducir el video.', error);
  }
}

function expandVideo(trigger) {
  if (videoShowcase.classList.contains('is-expanded')) {
    return;
  }

  lastVideoTrigger = trigger;
  videoShowcase.classList.add('is-expanded');
  document.body.classList.add('video-expanded');
  videoCloseButton.focus({ preventScroll: true });
  ensureVideoIsPlaying();
}

function closeExpandedVideo() {
  if (!videoShowcase.classList.contains('is-expanded')) {
    return;
  }

  videoShowcase.classList.remove('is-expanded');
  document.body.classList.remove('video-expanded');
  lastVideoTrigger?.focus({ preventScroll: true });
}

document.querySelectorAll('.js-video-trigger').forEach((trigger) => {
  trigger.addEventListener('click', () => expandVideo(trigger));
});

videoExpandButton.addEventListener('click', () => expandVideo(videoExpandButton));
videoCloseButton.addEventListener('click', closeExpandedVideo);

document.addEventListener('keydown', (event) => {
  if (event.key === 'Escape' && videoShowcase.classList.contains('is-expanded')) {
    closeExpandedVideo();
  }
});

videoPlayToggle.addEventListener('click', async () => {
  if (mindlinkVideo.paused) {
    await ensureVideoIsPlaying();
  } else {
    mindlinkVideo.pause();
  }
});

videoMuteButton.addEventListener('click', () => {
  mindlinkVideo.muted = !(mindlinkVideo.muted || mindlinkVideo.volume === 0);

  if (!mindlinkVideo.muted && mindlinkVideo.volume === 0) {
    mindlinkVideo.volume = 1;
  }

  updateMuteControl();
});

mindlinkVideo.addEventListener('volumechange', updateMuteControl);
mindlinkVideo.addEventListener('play', () => {
  updatePlayControl();
  startAnnotationSync();
});
mindlinkVideo.addEventListener('pause', () => {
  updatePlayControl();
  window.cancelAnimationFrame(annotationAnimationFrame);
});
mindlinkVideo.addEventListener('timeupdate', updateVideoComment);
mindlinkVideo.addEventListener('seeked', updateVideoComment);
mindlinkVideo.addEventListener('loadeddata', () => {
  updateVideoComment();
  ensureVideoIsPlaying();
});
updateMuteControl();
updatePlayControl();
updateVideoComment();
startAnnotationSync();
ensureVideoIsPlaying();

document.querySelector('.nav-toggle').addEventListener('click', (event) => {
  const links = document.querySelector('#nav-links');
  const expanded = links.classList.toggle('open');
  event.currentTarget.setAttribute('aria-expanded', expanded);
});

document.querySelectorAll('#nav-links a').forEach((link) => link.addEventListener('click', () => {
  document.querySelector('#nav-links').classList.remove('open');
  document.querySelector('.nav-toggle').setAttribute('aria-expanded', 'false');
}));

purposeInput.addEventListener('change', () => {
  const isOther = purposeInput.value === 'Otro';
  otherPurposeField.hidden = !isOther;
  otherPurposeInput.required = isOther;
  purposeError.textContent = '';
  otherPurposeError.textContent = '';

  if (isOther) {
    otherPurposeInput.focus();
  } else {
    otherPurposeInput.value = '';
  }
});

function validateForm() {
  emailError.textContent = '';
  purposeError.textContent = '';
  otherPurposeError.textContent = '';

  if (!emailInput.validity.valid) {
    emailError.textContent = 'Ingresa un correo electrónico válido para unirte a la lista.';
    emailInput.focus();
    return false;
  }

  if (!purposeInput.value) {
    purposeError.textContent = 'Selecciona para qué tipo de trabajo usarías MindLink.';
    purposeInput.focus();
    return false;
  }

  if (purposeInput.value === 'Otro' && !otherPurposeInput.value.trim()) {
    otherPurposeError.textContent = 'Escribe cuál sería el otro tipo de investigación.';
    otherPurposeInput.focus();
    return false;
  }

  return true;
}

function saveLocalPreview(formData) {
  const entries = JSON.parse(localStorage.getItem('mindlink-waitlist') || '[]');
  const email = String(formData.get('email')).trim().toLowerCase();

  if (!entries.some((entry) => entry.email === email)) {
    entries.push({
      name: String(formData.get('name') || '').trim(),
      email,
      purpose: formData.get('purpose') === 'Otro'
        ? String(formData.get('otherPurpose')).trim()
        : formData.get('purpose'),
      purposeCategory: formData.get('purpose'),
      registeredAt: new Date().toISOString()
    });
    localStorage.setItem('mindlink-waitlist', JSON.stringify(entries));
  }
}

async function submitWaitlist(formData) {
  const isLocalPreview = window.location.protocol === 'file:'
    || window.location.hostname === 'localhost'
    || window.location.hostname === '127.0.0.1';

  if (isLocalPreview) {
    saveLocalPreview(formData);
    return;
  }

  const payload = new URLSearchParams();
  formData.forEach((value, key) => payload.append(key, value));

  const response = await fetch('/', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: payload.toString()
  });

  if (!response.ok) {
    throw new Error(`Netlify respondió con el estado ${response.status}.`);
  }
}

form.addEventListener('submit', async (event) => {
  event.preventDefault();
  success.hidden = true;
  submissionError.hidden = true;
  submissionError.textContent = '';

  if (!validateForm()) {
    return;
  }

  const formData = new FormData(form);
  submitButton.disabled = true;
  submitButton.textContent = 'Reservando tu lugar…';
  form.setAttribute('aria-busy', 'true');

  try {
    await submitWaitlist(formData);
    form.querySelectorAll('input, select, button').forEach((element) => {
      element.disabled = true;
    });
    success.hidden = false;
    success.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
  } catch (error) {
    console.error(error);
    submissionError.textContent = 'No pudimos registrar tu correo. Revisa tu conexión e inténtalo nuevamente.';
    submissionError.hidden = false;
    submitButton.disabled = false;
    submitButton.innerHTML = submitButtonContent;
  } finally {
    form.removeAttribute('aria-busy');
  }
});
