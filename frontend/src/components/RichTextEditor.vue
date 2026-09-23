<template>
  <div class="border border-slate-300 rounded-xl overflow-hidden bg-slate-50 focus-within:ring-2 focus-within:ring-rose-500 focus-within:bg-white focus-within:border-rose-400 transition-all font-sans">
    
    <!-- Editor Toolbar -->
    <div class="flex flex-wrap items-center gap-1 p-2 bg-slate-100 border-b border-slate-200 text-slate-700 text-xs font-bold select-none">
      
      <!-- Formatting Buttons -->
      <button 
        type="button" 
        @click="exec('bold')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition font-bold text-sm"
        title="In đậm (Ctrl+B)"
      >
        B
      </button>
      
      <button 
        type="button" 
        @click="exec('italic')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition italic font-serif text-sm"
        title="In nghiêng (Ctrl+I)"
      >
        I
      </button>

      <button 
        type="button" 
        @click="exec('underline')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition underline text-sm"
        title="Gạch chân (Ctrl+U)"
      >
        U
      </button>

      <button 
        type="button" 
        @click="exec('strikeThrough')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition line-through text-sm"
        title="Gạch ngang"
      >
        S
      </button>

      <div class="w-px h-5 bg-slate-300 mx-1"></div>

      <!-- Lists -->
      <button 
        type="button" 
        @click="exec('insertUnorderedList')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition text-sm"
        title="Danh sách gạch đầu dòng"
      >
        • List
      </button>

      <button 
        type="button" 
        @click="exec('insertOrderedList')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition text-sm"
        title="Danh sách đánh số"
      >
        1. List
      </button>

      <div class="w-px h-5 bg-slate-300 mx-1"></div>

      <!-- Alignment -->
      <button 
        type="button" 
        @click="exec('justifyLeft')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition text-xs"
        title="Căn trái"
      >
        ⬅
      </button>

      <button 
        type="button" 
        @click="exec('justifyCenter')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition text-xs"
        title="Căn giữa"
      >
        ↔
      </button>

      <button 
        type="button" 
        @click="exec('justifyRight')" 
        class="w-7 h-7 flex items-center justify-center rounded hover:bg-slate-200 transition text-xs"
        title="Căn phải"
      >
        ➡
      </button>

      <div class="w-px h-5 bg-slate-300 mx-1"></div>

      <!-- Text Colors -->
      <div class="flex items-center gap-1">
        <button 
          type="button" 
          @click="exec('foreColor', '#1e293b')" 
          class="w-4 h-4 rounded-full bg-slate-800 border border-slate-300 hover:scale-110 transition"
          title="Màu mặc định"
        ></button>
        <button 
          type="button" 
          @click="exec('foreColor', '#e11d48')" 
          class="w-4 h-4 rounded-full bg-rose-600 border border-rose-300 hover:scale-110 transition"
          title="Màu đỏ nổi bật"
        ></button>
        <button 
          type="button" 
          @click="exec('foreColor', '#2563eb')" 
          class="w-4 h-4 rounded-full bg-blue-600 border border-blue-300 hover:scale-110 transition"
          title="Màu xanh dương"
        ></button>
      </div>

      <div class="w-px h-5 bg-slate-300 mx-1"></div>

      <!-- Clear Formatting -->
      <button 
        type="button" 
        @click="exec('removeFormat')" 
        class="px-2 py-1 rounded hover:bg-slate-200 transition text-[11px] text-slate-600"
        title="Xóa định dạng"
      >
        🧹 Xóa định dạng
      </button>

      <!-- Restore Draft Template Button -->
      <button 
        v-if="showRestoreBtn"
        type="button" 
        @click="$emit('restoreTemplate')" 
        class="ml-auto px-2.5 py-1 rounded-lg bg-rose-50 text-rose-700 hover:bg-rose-100 border border-rose-200 transition text-[11px] font-bold flex items-center gap-1 shadow-2xs"
      >
        ⚡ Khôi phục văn bản mẫu
      </button>
    </div>

    <!-- Content Editable Workspace -->
    <div 
      ref="editorRef"
      contenteditable="true"
      @input="onInput"
      @blur="onInput"
      class="w-full min-h-[220px] max-h-[360px] overflow-y-auto p-4 text-xs text-slate-800 font-medium leading-relaxed outline-none space-y-1 bg-transparent border-none custom-scrollbar"
    ></div>

  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';

const props = defineProps({
  modelValue: { type: String, default: '' },
  showRestoreBtn: { type: Boolean, default: true }
});

const emit = defineEmits(['update:modelValue', 'restoreTemplate']);

const editorRef = ref(null);

function exec(command, value = null) {
  document.execCommand(command, false, value);
  onInput();
}

function onInput() {
  if (editorRef.value) {
    emit('update:modelValue', editorRef.value.innerHTML);
  }
}

watch(() => props.modelValue, (newVal) => {
  if (editorRef.value && editorRef.value.innerHTML !== newVal) {
    editorRef.value.innerHTML = newVal || '';
  }
});

onMounted(() => {
  if (editorRef.value) {
    editorRef.value.innerHTML = props.modelValue || '';
  }
});
</script>

<style scoped>
:deep(ul) {
  list-style-type: disc;
  padding-left: 1.25rem;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(ol) {
  list-style-type: decimal;
  padding-left: 1.25rem;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(p) {
  margin-bottom: 0.25rem;
}
</style>
