function groupElementsByRow(elements) {
  const rows = [];

  elements.forEach((element) => {
    const top = Math.round(element.getBoundingClientRect().top + window.scrollY);
    const currentRow = rows[rows.length - 1];

    if (!currentRow || Math.abs(currentRow.top - top) > 10) {
      rows.push({ top, elements: [element] });
      return;
    }

    currentRow.elements.push(element);
  });

  return rows;
}

function applyEqualHeights(selector) {
  const elements = Array.from(document.querySelectorAll(selector));
  if (!elements.length) {
    return;
  }

  elements.forEach((element) => {
    element.style.minHeight = '';
  });

  groupElementsByRow(elements).forEach((row) => {
    const maxHeight = Math.max(...row.elements.map((element) => element.offsetHeight));
    row.elements.forEach((element) => {
      element.style.minHeight = `${maxHeight}px`;
    });
  });
}

function initializeEqualHeights() {
  const updateHeights = () => {
    applyEqualHeights('#cards-wrapper .item-inner');
    applyEqualHeights('#showcase .card');
  };

  updateHeights();
  window.addEventListener('resize', updateHeights);
  window.addEventListener('load', updateHeights);
}

function initializeSmoothScroll() {
  document.querySelectorAll('a.scrollto[href^="#"]').forEach((link) => {
    link.addEventListener('click', (event) => {
      const href = link.getAttribute('href');
      if (!href) {
        return;
      }

      const target = document.querySelector(href);
      if (!target) {
        return;
      }

      event.preventDefault();
      target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      history.replaceState(null, '', href);
    });
  });
}

function initializeDocNavigation() {
  const links = Array.from(document.querySelectorAll('#doc-nav a.scrollto[href^="#"]'));
  const sections = links
    .map((link) => {
      const href = link.getAttribute('href');
      return href ? document.querySelector(href) : null;
    })
    .filter((section) => section instanceof HTMLElement);

  if (!links.length || !sections.length) {
    return;
  }

  const activateLink = (sectionId) => {
    document.querySelectorAll('#doc-nav li').forEach((item) => item.classList.remove('active'));

    links.forEach((link) => {
      const href = link.getAttribute('href');
      if (href !== `#${sectionId}`) {
        return;
      }

      link.closest('li')?.classList.add('active');
      const parentMenuItem = link.closest('.doc-sub-menu')?.closest('li');
      parentMenuItem?.classList.add('active');
    });
  };

  const observer = new IntersectionObserver(
    (entries) => {
      const visibleEntries = entries
        .filter((entry) => entry.isIntersecting)
        .sort((left, right) => right.intersectionRatio - left.intersectionRatio);

      if (visibleEntries.length) {
        activateLink(visibleEntries[0].target.id);
      }
    },
    {
      threshold: [0.15, 0.4, 0.7],
      rootMargin: '-120px 0px -55% 0px'
    }
  );

  sections.forEach((section) => observer.observe(section));

  const initialHash = window.location.hash.slice(1);
  if (initialHash) {
    activateLink(initialHash);
  } else if (sections[0]) {
    activateLink(sections[0].id);
  }
}

function closeLightbox() {
  const overlay = document.querySelector('.site-lightbox');
  if (!overlay) {
    return;
  }

  overlay.classList.remove('is-visible');
  document.body.classList.remove('lightbox-open');
  setTimeout(() => overlay.remove(), 200);
}

function openLightbox(href, title) {
  let overlay = document.querySelector('.site-lightbox');

  if (!overlay) {
    overlay = document.createElement('div');
    overlay.className = 'site-lightbox';
    overlay.innerHTML = `
      <button class="site-lightbox__close" type="button" aria-label="Close image viewer">&times;</button>
      <figure class="site-lightbox__figure">
        <img class="site-lightbox__image" alt="">
        <figcaption class="site-lightbox__caption"></figcaption>
      </figure>
    `;

    overlay.addEventListener('click', (event) => {
      if (event.target === overlay || event.target.closest('.site-lightbox__close')) {
        closeLightbox();
      }
    });

    document.addEventListener('keydown', (event) => {
      if (event.key === 'Escape') {
        closeLightbox();
      }
    });

    document.body.appendChild(overlay);
  }

  const image = overlay.querySelector('.site-lightbox__image');
  const caption = overlay.querySelector('.site-lightbox__caption');

  if (!(image instanceof HTMLImageElement) || !(caption instanceof HTMLElement)) {
    return;
  }

  image.src = href;
  image.alt = title || '';
  caption.textContent = title || '';
  document.body.classList.add('lightbox-open');
  requestAnimationFrame(() => overlay.classList.add('is-visible'));
}

function initializeLightbox() {
  document.querySelectorAll('[data-toggle="lightbox"]').forEach((trigger) => {
    trigger.addEventListener('click', (event) => {
      const link = event.currentTarget;
      if (!(link instanceof HTMLAnchorElement)) {
        return;
      }

      event.preventDefault();
      openLightbox(link.href, link.dataset.title || link.querySelector('img')?.alt || '');
    });
  });
}

document.addEventListener('DOMContentLoaded', () => {
  initializeEqualHeights();
  initializeSmoothScroll();
  initializeDocNavigation();
  initializeLightbox();
});
