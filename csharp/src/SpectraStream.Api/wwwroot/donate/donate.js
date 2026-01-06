document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('donateForm');
    const submitBtn = document.getElementById('submitBtn');
    const status = document.getElementById('status');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const donor = document.getElementById('donor').value.trim() || 'Anonymous';
        const amount = parseFloat(document.getElementById('amount').value);
        const message = document.getElementById('message').value.trim();

        if (!amount || amount <= 0) {
            showStatus('Please enter a valid amount', 'error');
            return;
        }

        submitBtn.disabled = true;
        submitBtn.textContent = 'Sending...';

        try {
            const response = await fetch('/api/donation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    time: new Date().toISOString(),
                    donor: donor,
                    amount: amount,
                    message: message
                })
            });

            if (response.ok) {
                showStatus('Thank you for your donation!', 'success');
                form.reset();
                document.getElementById('amount').value = '5.00';
            } else {
                showStatus('Something went wrong. Please try again.', 'error');
            }
        } catch (err) {
            showStatus('Could not connect. Please try again.', 'error');
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = 'Send Donation';
        }
    });

    function showStatus(msg, type) {
        status.textContent = msg;
        status.className = `status ${type}`;
    }
});
