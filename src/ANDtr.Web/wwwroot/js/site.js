// ANDtr Medical Device Safety Platform - Client Interactivity
(function () {
  "use strict";

  // 1. Selection Management (Survives pagination via sessionStorage)
  var STORAGE_KEY = "andtr_selected_notices";

  function getSelectedNotices() {
    try {
      var raw = sessionStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch (e) {
      return [];
    }
  }

  function saveSelectedNotices(ids) {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
    } catch (e) {}
  }

  function updateSelectionUi() {
    var selected = getSelectedNotices();
    var countElements = document.querySelectorAll("#selCount, .js-sel-count");
    countElements.forEach(function (el) {
      el.textContent = selected.length;
    });

    // Sync checkboxes currently rendered on the page
    var pageCheckboxes = document.querySelectorAll(".sel");
    var allChecked = pageCheckboxes.length > 0;

    pageCheckboxes.forEach(function (cb) {
      var id = parseInt(cb.getAttribute("data-id") || cb.value, 10);
      if (selected.indexOf(id) !== -1) {
        cb.checked = true;
      } else {
        cb.checked = false;
        allChecked = false;
      }
    });

    var selectAll = document.getElementById("selAll");
    if (selectAll && pageCheckboxes.length > 0) {
      selectAll.checked = allChecked;
    }

    // Sync any hidden inputs for "Add to Risk Review" form
    var hiddenInputsContainer = document.getElementById("selectedNoticesContainer");
    if (hiddenInputsContainer) {
      hiddenInputsContainer.innerHTML = "";
      selected.forEach(function (id) {
        var input = document.createElement("input");
        input.type = "hidden";
        input.name = "noticeIds";
        input.value = id;
        hiddenInputsContainer.appendChild(input);
      });
    }

    var addBtn = document.querySelector(".js-add-to-review");
    if (addBtn) {
      if (selected.length === 0) {
        addBtn.classList.add("disabled");
        addBtn.setAttribute("aria-disabled", "true");
      } else {
        addBtn.classList.remove("disabled");
        addBtn.removeAttribute("aria-disabled");
      }
    }
  }

  // 2. Global Event Delegations
  document.addEventListener("DOMContentLoaded", function () {
    updateSelectionUi();

    // Checkbox changes
    document.addEventListener("change", function (e) {
      var t = e.target;
      if (t.id === "selAll") {
        var selected = getSelectedNotices();
        var pageBoxes = document.querySelectorAll(".sel");
        pageBoxes.forEach(function (cb) {
          var id = parseInt(cb.getAttribute("data-id") || cb.value, 10);
          if (t.checked) {
            if (selected.indexOf(id) === -1) selected.push(id);
          } else {
            selected = selected.filter(function (x) { return x !== id; });
          }
        });
        saveSelectedNotices(selected);
        updateSelectionUi();
      } else if (t.classList && t.classList.contains("sel")) {
        var selected = getSelectedNotices();
        var id = parseInt(t.getAttribute("data-id") || t.value, 10);
        if (t.checked) {
          if (selected.indexOf(id) === -1) selected.push(id);
        } else {
          selected = selected.filter(function (x) { return x !== id; });
        }
        saveSelectedNotices(selected);
        updateSelectionUi();
      }
    });

    // Click events
    document.addEventListener("click", function (e) {
      var t = e.target;

      // Clear selection
      if (t.closest(".js-clearsel")) {
        saveSelectedNotices([]);
        updateSelectionUi();
        return;
      }

      // Print button
      if (t.closest(".js-print")) {
        window.print();
        return;
      }

      // Modal open
      var opener = t.closest("[data-open]");
      if (opener) {
        var targetId = opener.getAttribute("data-open");
        var modal = document.getElementById(targetId);
        if (modal) modal.hidden = false;
        return;
      }

      // Modal close
      if (t.closest("[data-close]") || t.classList.contains("modal-overlay")) {
        var modal = t.closest(".modal-overlay");
        if (modal) modal.hidden = true;
        return;
      }

      // Rich text toolbar formatting
      var cmdBtn = t.closest(".tb button[data-cmd]");
      if (cmdBtn) {
        var cmd = cmdBtn.getAttribute("data-cmd");
        document.execCommand(cmd, false, null);
        var ed = cmdBtn.closest(".editor")?.querySelector(".ed");
        if (ed) ed.focus();
        return;
      }
    });

    // Mousedown on toolbar buttons (preserves selection in editor)
    document.addEventListener("mousedown", function (e) {
      if (e.target.closest(".tb button")) {
        e.preventDefault();
      }
    });

    // Sync contentEditable to hidden textarea before form submission
    var formsWithEditor = document.querySelectorAll("form:has(.editor)");
    formsWithEditor.forEach(function (form) {
      form.addEventListener("submit", function () {
        var ed = form.querySelector(".ed");
        var targetTextarea = form.querySelector(".js-editor-target");
        if (ed && targetTextarea) {
          targetTextarea.value = ed.innerHTML;
        }
      });
    });

    // Generate random secure password helper for customer creation
    var genBtn = document.getElementById("btnGenPassword");
    if (genBtn) {
      genBtn.addEventListener("click", function () {
        var upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        var lower = "abcdefghijkmnopqrstuvwxyz";
        var digits = "23456789";
        var spec = "!@#$%^&*";
        var all = upper + lower + digits + spec;

        var pwd = [
          upper[Math.floor(Math.random() * upper.Length) || 0],
          lower[Math.floor(Math.random() * lower.length) || 0],
          digits[Math.floor(Math.random() * digits.length) || 0],
          spec[Math.floor(Math.random() * spec.length) || 0]
        ];

        for (var i = 4; i < 10; i++) {
          pwd.push(all[Math.floor(Math.random() * all.length)]);
        }

        var res = pwd.sort(function () { return 0.5 - Math.random(); }).join("");
        var p1 = document.getElementById("cPw");
        var p2 = document.getElementById("cPw2");
        if (p1) p1.value = res;
        if (p2) p2.value = res;
      });
    }
  });
})();
